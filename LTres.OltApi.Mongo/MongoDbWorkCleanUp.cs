using System.Diagnostics;
using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace LTres.OltApi.Mongo;

public class MongoDbWorkCleanUp : IDbWorkCleanUp
{
    private ILogger _log;
    private IMongoCollection<OLT_Host_Item> OLT_Host_Items;
    private IMongoCollection<OLT_Host_Item_History> OLT_Host_Items_History;
    private PipelineDefinition<OLT_Host_Item, CleanUpItemHistory> pipelineDefinitionItemsHistoryToDelete;
    private PipelineDefinition<OLT_Host_Item, CleanUpItemHistory> pipelineDefinitionItemsNestedToDelete;
    private PipelineDefinition<OLT_Host_Item_History, CleanUpItemHistory> pipelineDefinitionHistoryItemsOrphansToDelete;

    public MongoDbWorkCleanUp(IOptions<MongoConfig> options, ILogger<MongoDbWorkCleanUp> logger)
    {
        _log = logger;

        var config = options.Value;
        var client = new MongoClient(config.ConnectionString);
        var database = client.GetDatabase(config.DatabaseName);

        OLT_Host_Items = database.GetCollection<OLT_Host_Item>("olt_host_items");
        OLT_Host_Items_History = database.GetCollection<OLT_Host_Item_History>("olt_host_items_history");

        pipelineDefinitionItemsHistoryToDelete = PipelineDefinition<OLT_Host_Item, CleanUpItemHistory>.Create(
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "olt_host_items" },
                { "localField", "IdRelated" },
                { "foreignField", "_id" },
                { "as", "Related" }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 1 },
                { "HistoryFor",
                    new BsonDocument("$max",
                    new BsonArray
                    {
                        0,
                        "$HistoryFor",
                        new BsonDocument("$arrayElemAt", new BsonArray { "$Related.HistoryFor", 0 })
                    })
                }
            }),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "olt_host_items_history" },
                { "localField", "_id" },
                { "foreignField", "IdItem" },
                { "as", "History" }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 1 },
                { "Until", new BsonDocument("$subtract",
                    new BsonArray
                    {
                        "$$NOW",
                        new BsonDocument("$multiply", new BsonArray { "$HistoryFor", 60000 })
                    })
                },
                { "History", new BsonDocument("$size", "$History") }
            }),
            new BsonDocument("$match", new BsonDocument("History", new BsonDocument("$gt", 0))));



        pipelineDefinitionItemsNestedToDelete = PipelineDefinition<OLT_Host_Item, CleanUpItemHistory>.Create(
            new BsonDocument("$match", new BsonDocument("Action", "snmpwalk")),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "olt_host_items" },
                { "localField", "_id" },
                { "foreignField", "IdRelated" },
                { "as", "Adjacent" }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "History", new BsonDocument("$size", "$Adjacent") },
                { "Until", new BsonDocument("$subtract", new BsonArray {
                    "$LastProbed",
                    new BsonDocument("$multiply", new BsonArray
                    {
                        new BsonDocument("$max", new BsonArray { 1, "$MaintainFor" }),
                        60000
                    })
                }) }
            }),
            new BsonDocument("$match", new BsonDocument("History", new BsonDocument("$gt", 0))));


        pipelineDefinitionHistoryItemsOrphansToDelete = PipelineDefinition<OLT_Host_Item_History, CleanUpItemHistory>.Create(
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "olt_host_items" }, 
                { "localField", "IdItem" }, 
                { "foreignField", "_id" }, 
                { "as", "ParentItem" }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 1 }, 
                { "IdItem", 1 },
                { "Until", "$$NOW" }, 
                { "History", new BsonDocument("$size", "$ParentItem") }
            }),
            new BsonDocument("$match", new BsonDocument("History",  new BsonDocument("$lte", 0))),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$IdItem" }, 
                { "Until", new BsonDocument("$max", "$Until") }, 
                { "History", new BsonDocument("$max", 1) }
            }));
    }

    public async Task<long> CleanUpExecute()
    {
        long iTotalItemsRemoved = 0;

        iTotalItemsRemoved += await RemoveRelatedExpiredItems();

        iTotalItemsRemoved += await RemoveHistoryExpiredItems();        

        return iTotalItemsRemoved;
    }

    private async Task<long> RemoveRelatedExpiredItems()
    {
        long removedCount = 0;
        var timer = Stopwatch.StartNew();

        var listItemsNestedToDelete = await OLT_Host_Items
            .Aggregate(pipelineDefinitionItemsNestedToDelete)
            .ToListAsync();

        foreach(var toDel in listItemsNestedToDelete)
        {
            var deleteResult = await OLT_Host_Items
                .DeleteManyAsync(f => f.IdRelated == toDel.Id && f.LastProbed <= toDel.Until);

            if (deleteResult.IsAcknowledged)
                removedCount += deleteResult.DeletedCount;
        }

        timer.Stop();
        _log.LogDebug($"Remove related expired items executed in {timer.Elapsed}");
        return removedCount;
    }

    private async Task<long> RemoveHistoryExpiredItems()
    {
        long removedCount = 0;
        var timer = Stopwatch.StartNew();

        var listItemsHistoryToDelete = await OLT_Host_Items
            .Aggregate(pipelineDefinitionItemsHistoryToDelete)
            .ToListAsync();

        foreach (var toDel in listItemsHistoryToDelete)
        {
            var deleteResult = await OLT_Host_Items_History
                .DeleteManyAsync(f => f.IdItem == toDel.Id && f.At <= toDel.Until);

            if (deleteResult.IsAcknowledged)
                removedCount += deleteResult.DeletedCount;
        }

        timer.Stop();
        _log.LogDebug($"Remove history expired items executed in {timer.Elapsed}");

        timer.Start();
        var listItemsHistoryOphansToDelete = await OLT_Host_Items_History
            .Aggregate(pipelineDefinitionHistoryItemsOrphansToDelete)
            .ToListAsync();

        foreach (var toDel in listItemsHistoryOphansToDelete)
        {
            var deleteResult = await OLT_Host_Items_History
                .DeleteManyAsync(f => f.IdItem == toDel.Id);

            if (deleteResult.IsAcknowledged)
                removedCount += deleteResult.DeletedCount;
        }

        timer.Stop();
        _log.LogDebug($"Remove items history orphans executed in {timer.Elapsed}");

        return removedCount;
    }

}

class CleanUpItemHistory
{
    [BsonId]
    public Guid Id { get; set; }
    public DateTime Until { get; set; }
    public int? History { get; set; }
}