using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Common.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LTres.OltApi.Mongo;

public class MongoDbWorkProbeInfo : IDbWorkProbeInfo
{
    private IMongoCollection<OLT_Host_Item> OLT_Host_Items;
    private PipelineDefinition<OLT_Host_Item, WorkProbeInfo> pipelineDefinitionWorkProbeInfo;

    public MongoDbWorkProbeInfo(IOptions<MongoConfig> options)
    {
        var config = options.Value;
        var client = new MongoClient(config.ConnectionString);
        var database = client.GetDatabase(config.DatabaseName);

        OLT_Host_Items = database.GetCollection<OLT_Host_Item>("olt_host_items");

        //create a pipeline to get only new work to be done
        pipelineDefinitionWorkProbeInfo = PipelineDefinition<OLT_Host_Item, WorkProbeInfo>.Create(
            new BsonDocument("$match", new BsonDocument
            {
                { "Active", true },
                { "IdOltHost", new BsonDocument("$ne", BsonNull.Value) },
                { "Action", new BsonDocument("$ne", BsonNull.Value) },
                { "Template", new BsonDocument("$ne", true) }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 1 },
                { "IdOltHost", 1 },
                { "IdRelated", 1 },
                { "Action", 1 },
                { "ItemKey", 1},
                { "LastProbed", 1 },
                { "Calc", 1 },
                { "AsHex", 1 },
                { "HistoryFor", 1 },
                { "DoProbe",
                    new BsonDocument("$gte", new BsonArray
                    {
                        "$$NOW",
                        new BsonDocument("$add", new BsonArray
                            {
                                "$LastProbed",
                                new BsonDocument("$multiply", new BsonArray { "$Interval", 1000 })
                            })
                    })
                }
            }),
            new BsonDocument("$match", new BsonDocument
            {
                { "DoProbe", true }
            }),
            new BsonDocument("$limit", 1000),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "olt_hosts" },
                { "localField", "IdOltHost" },
                { "foreignField", "_id" },
                { "as", "olt" }
            }),
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
                { "Action", 1 },
                { "ItemKey", new BsonDocument("$switch",
                    new BsonDocument {
                    { "branches", new BsonArray {
                        new BsonDocument {
                            { "case", new BsonDocument("$gt", new BsonArray { "$IdRelated", BsonNull.Value }) },
                            { "then", new BsonDocument("$replaceOne", new BsonDocument {
                                { "input", new BsonDocument("$arrayElemAt", new BsonArray { "$Related.ItemKey", 0 }) },
                                { "find", "{index}" },
                                { "replacement", "$ItemKey" }
                            }) }
                    }}},
                    { "default", "$ItemKey" }
                })},
                { "LastProbed", 1 },
                { "Calc", new BsonDocument("$max", new BsonArray {
                    "$Calc",
                    new BsonDocument("$arrayElemAt", new BsonArray { "$Related.Calc", 0 }) })
                },
                { "AsHex", new BsonDocument("$max", new BsonArray {
                    "$AsHex",
                    new BsonDocument("$arrayElemAt", new BsonArray { "$Related.AsHex", 0 }) })
                },
                { "DoHistory", new BsonDocument("$gt", new BsonArray {
                    new BsonDocument("$max", new BsonArray { "$HistoryFor", new BsonDocument("$arrayElemAt", new BsonArray { "$Related.HistoryFor", 0 }) })
                    , 0 })
                },
                { "Host", new BsonDocument("$arrayElemAt", new BsonArray { "$olt.Host", 0 }) },
                { "SnmpCommunity", new BsonDocument("$arrayElemAt", new BsonArray { "$olt.SnmpCommunity", 0 }) },
                { "SnmpVersion", new BsonDocument("$arrayElemAt", new BsonArray { "$olt.SnmpVersion", 0 }) },
                { "SnmpBulk", new BsonDocument("$arrayElemAt", new BsonArray { "$olt.SnmpBulk", 0 }) }
            }));
    }

    public async Task<IEnumerable<WorkProbeInfo>> ToDoList() =>
        await OLT_Host_Items
            .Aggregate(pipelineDefinitionWorkProbeInfo)
            .ToListAsync();
}