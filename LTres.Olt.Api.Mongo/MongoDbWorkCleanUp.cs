using System.Diagnostics;
using LTres.Olt.Api.Common.DbServices;
using LTres.Olt.Api.Common.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace LTres.Olt.Api.Mongo;

public class MongoDbWorkCleanUp(IMongoDatabase database, ILogger<MongoDbWorkCleanUp> logger) : IDbWorkCleanUp
{
    private readonly ILogger _log = logger;
    private readonly IMongoCollection<OLT_Host_Item> OLT_Host_Items = database.GetCollection<OLT_Host_Item>("olt_host_items");
    private readonly IMongoCollection<IdModel> to_remove_olt_items = database.GetCollection<IdModel>("to_remove_olt_items");
    private readonly IMongoCollection<OLT_Host_Item_History> OLT_Host_Items_History = database.GetCollection<OLT_Host_Item_History>("olt_host_items_history");
    private readonly PipelineDefinition<OLT_Host_Item, CleanUpItemHistory> pipelineDefinitionItemsHistoryToDelete = CreatePipelineDefinitionItemsHistoryToDelete();
    private readonly PipelineDefinition<OLT_Host_Item_History, CleanUpItemHistory> pipelineDefinitionHistoryItemsOrphansToDelete = CreatePipelineDefinitionHistoryItemsOrphansToDelete();

    internal static PipelineDefinition<OLT_Host_Item, CleanUpItemHistory> CreatePipelineDefinitionItemsHistoryToDelete() => PipelineDefinition<OLT_Host_Item, CleanUpItemHistory>.Create(
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

    internal static PipelineDefinition<OLT_Host_Item_History, CleanUpItemHistory> CreatePipelineDefinitionHistoryItemsOrphansToDelete() => PipelineDefinition<OLT_Host_Item_History, CleanUpItemHistory>.Create(
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
        new BsonDocument("$match", new BsonDocument("History", new BsonDocument("$lte", 0))),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$IdItem" },
            { "Until", new BsonDocument("$max", "$Until") },
            { "History", new BsonDocument("$max", 1) }
        }));


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

        var listItemsNestedToDelete = (await to_remove_olt_items.FindAsync(_ => true)).ToList();

        foreach (var toDel in listItemsNestedToDelete)
        {
            var deleteResult = await OLT_Host_Items
                .DeleteOneAsync(f => f.Id == toDel.Id);

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
        var listItemsHistoryOrphansToDelete = await OLT_Host_Items_History
            .Aggregate(pipelineDefinitionHistoryItemsOrphansToDelete)
            .ToListAsync();

        foreach (var toDel in listItemsHistoryOrphansToDelete)
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

    public class IdModel
    {
        [BsonId]
        public Guid Id { get; set; }
    }

    public class CleanUpItemHistory
    {
        [BsonId]
        public Guid Id { get; set; }
        public DateTime Until { get; set; }
        public int? History { get; set; }
    }
}