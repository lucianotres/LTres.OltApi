using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Amazon.Runtime.SharedInterfaces;
using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Common.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Linq;

namespace LTres.OltApi.Mongo;

public static class MongoDbOltApiMigrations
{

    public static async Task Do(MongoConfig config)
    {
        var client = new MongoClient(config.ConnectionString);
        var database = client.GetDatabase(config.DatabaseName);

        await CheckOrCreateCollections(database);
        await CheckOrCreateIndexes(database);
        await CheckOrCreateView(database);
    }

    private static async Task CheckOrCreateCollections(IMongoDatabase database)
    {
        var collections = (await database.ListCollectionNamesAsync()).ToList();

        if (!collections.Any(s => s == "olt_hosts"))
            await database.CreateCollectionAsync("olt_hosts");

        if (!collections.Any(s => s == "olt_host_items"))
            await database.CreateCollectionAsync("olt_host_items");

        if (!collections.Any(s => s == "olt_host_items_history"))
            await database.CreateCollectionAsync("olt_host_items_history");

    }

    private static async Task CheckOrCreateIndexes(IMongoDatabase database)
    {
        var collectionOltHostItems = database.GetCollection<OLT_Host_Item>("olt_host_items");
        var indexesOltHostItems = (await collectionOltHostItems.Indexes.ListAsync()).ToList();
        if (!indexesOltHostItems.Any(i => i.FirstOrDefault(f => f.Name == "name").Value.AsString == "idx_IdOltHost"))
        {
            var keys = Builders<OLT_Host_Item>.IndexKeys
                .Ascending(p => p.IdOltHost)
                .Ascending(p => p.ItemKey);
            var createIndexModel = new CreateIndexModel<OLT_Host_Item>(keys, new CreateIndexOptions() { Name = "idx_IdOltHost" });

            await collectionOltHostItems.Indexes.CreateOneAsync(createIndexModel);
        }
        if (!indexesOltHostItems.Any(i => i.FirstOrDefault(f => f.Name == "name").Value.AsString == "idx_IdRelated"))
        {
            var keys = Builders<OLT_Host_Item>.IndexKeys
                .Ascending(p => p.IdRelated)
                .Ascending(p => p.ItemKey);
            var createIndexModel = new CreateIndexModel<OLT_Host_Item>(keys, new CreateIndexOptions() { Name = "idx_IdRelated" });

            await collectionOltHostItems.Indexes.CreateOneAsync(createIndexModel);
        }
        if (!indexesOltHostItems.Any(i => i.FirstOrDefault(f => f.Name == "name").Value.AsString == "idx_to_probe"))
        {
            var keys = Builders<OLT_Host_Item>.IndexKeys
                .Descending(p => p.Active)
                .Ascending(p => p.IdOltHost)
                .Ascending(p => p.Action)
                .Ascending(p => p.Template);
            var createIndexModel = new CreateIndexModel<OLT_Host_Item>(keys, new CreateIndexOptions() { Name = "idx_to_probe" });

            await collectionOltHostItems.Indexes.CreateOneAsync(createIndexModel);
        }

        var collectionOltHostItemsHistory = database.GetCollection<OLT_Host_Item_History>("olt_host_items_history");
        var indexesOltHostItemsHistory = (await collectionOltHostItemsHistory.Indexes.ListAsync()).ToList();
        if (!indexesOltHostItemsHistory.Any(i => i.FirstOrDefault(f => f.Name == "name").Value.AsString == "idx_IdItem"))
        {
            var keys = Builders<OLT_Host_Item_History>.IndexKeys.Ascending(p => p.IdItem).Ascending(p => p.At);
            var createIndexModel = new CreateIndexModel<OLT_Host_Item_History>(keys, new CreateIndexOptions() { Name = "idx_IdItem" });

            await collectionOltHostItemsHistory.Indexes.CreateOneAsync(createIndexModel);
        }
    }


    private static async Task CheckOrCreateView(IMongoDatabase database)
    {
        var collections = (await database.ListCollectionNamesAsync()).ToList();

        if (!collections.Any(s => s == "to_probe"))
        {
            //create a pipeline to get only new work to be done
            var pipelineDefinitionWorkProbeInfo = PipelineDefinition<OLT_Host_Item, WorkProbeInfo>.Create(
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
                { "SnmpBulk", new BsonDocument("$arrayElemAt", new BsonArray { "$olt.SnmpBulk", 0 }) },
                { "GetTimeout", new BsonDocument("$arrayElemAt", new BsonArray { "$olt.GetTimeout", 0 }) },
                }));


            await database.CreateViewAsync("to_probe", "olt_host_items", pipelineDefinitionWorkProbeInfo);
        }
    }
}