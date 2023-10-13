using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime.SharedInterfaces;
using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Common.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

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
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 1 },
                { "IdOltHost", 1 },
                { "Action", 1 },
                { "ItemKey", 1},
                { "LastProbed", 1 },
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
            new BsonDocument("$limit", 100),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "olt_hosts" },
                { "localField", "IdOltHost" },
                { "foreignField", "_id" },
                { "as", "olt" }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 1 },
                { "Action", 1 },
                { "ItemKey", 1},
                { "LastProbed", 1 },
                { "Host", new BsonDocument("$arrayElemAt", new BsonArray { "$olt.Host", 0 }) },
                { "SnmpCommunity", new BsonDocument("$arrayElemAt", new BsonArray { "$olt.SnmpCommunity", 0 }) }
            }));
    }

    public async Task<IEnumerable<WorkProbeInfo>> ToDoList() =>
        await OLT_Host_Items
            .Aggregate(pipelineDefinitionWorkProbeInfo)
            .ToListAsync();
}