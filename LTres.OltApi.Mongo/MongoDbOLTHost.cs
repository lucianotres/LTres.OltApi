using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Common.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace LTres.OltApi.Mongo;

public class MongoDbOLTHost : IDbOLTHost
{
    public MongoDbOLTHost(IOptions<MongoConfig> options)
    {
        var config = options.Value;
        var client = new MongoClient(config.ConnectionString);
        var database = client.GetDatabase(config.DatabaseName);

        OLT_Hosts = database.GetCollection<OLT_Host>("olt_hosts");
    }

    private IMongoCollection<OLT_Host> OLT_Hosts;

    public async Task<Guid> AddOLTHost(OLT_Host olt)
    {
        await OLT_Hosts.InsertOneAsync(olt);
        return olt.Id;
    }
}