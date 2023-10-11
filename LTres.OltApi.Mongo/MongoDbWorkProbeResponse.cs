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

public class MongoDbWorkProbeResponse : IDbWorkProbeResponse
{
    private IMongoCollection<OLT_Host_Item> OLT_Host_Items;
    private IMongoCollection<OLT_Host_Item_History> OLT_Host_Items_History;

    public MongoDbWorkProbeResponse(IOptions<MongoConfig> options)
    {
        var config = options.Value;
        var client = new MongoClient(config.ConnectionString);
        var database = client.GetDatabase(config.DatabaseName);

        OLT_Host_Items = database.GetCollection<OLT_Host_Item>("olt_host_items");
        OLT_Host_Items_History = database.GetCollection<OLT_Host_Item_History>("olt_host_items_history");
    }

    public async Task SaveWorkProbeResponse(WorkProbeResponse workProbeResponse)
    {
        var filter = Builders<OLT_Host_Item>.Filter.Eq(f => f.Id, workProbeResponse.Id);
        var update = Builders<OLT_Host_Item>.Update
            .Set(p => p.LastProbed, workProbeResponse.ProbedAt)
            .Set(p => p.ProbedSuccess, workProbeResponse.Success)
            .Set(p => p.ProbedValueInt, workProbeResponse.ValueInt)
            .Set(p => p.ProbedValueStr, workProbeResponse.ValueStr);

        await OLT_Host_Items_History.InsertOneAsync(OLT_Host_Item_History.From(workProbeResponse));
        await OLT_Host_Items.UpdateOneAsync(filter, update);
    }


}