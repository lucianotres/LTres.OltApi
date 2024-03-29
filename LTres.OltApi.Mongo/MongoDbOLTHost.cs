using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Common.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

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

    public async Task<int> ChangeOLTHost(OLT_Host olt)
    {
        var filter = Builders<OLT_Host>.Filter.Eq(o => o.Id, olt.Id);
        var result = await OLT_Hosts.ReplaceOneAsync(filter, olt);
        
        return result.IsAcknowledged && result.IsModifiedCountAvailable ? (int)result.ModifiedCount : 0;
    }

    public async Task<IEnumerable<OLT_Host>> ListOLTHosts(int take = 1000, int skip = 0,
        bool? filterActive = null,
        Guid? filterId = null,
        string? filterName = null,
        string? filterHost = null,
        string[]? filterTag = null)
    {
        var builder = Builders<OLT_Host>.Filter;
        var filters = new List<FilterDefinition<OLT_Host>>();

        //mount filters..
        if (filterActive.HasValue)
            filters.Add(builder.Eq(o => o.Active, filterActive.Value));
        if (filterId.HasValue)
            filters.Add(builder.Eq(o => o.Id, filterId.Value));
        if (filterName != null)
            filters.Add(builder.Regex(o => o.Name, filterName));
        if (filterHost != null)
            filters.Add(builder.Regex(o => o.Host, filterHost));
        if (filterTag != null)
            foreach (var tag in filterTag)
                filters.Add(builder.AnyEq(o => o.tags, tag));

        //query data..
        return await OLT_Hosts
            .Find(filters.Count > 1 ? builder.And(filters) : filters.Count == 1 ? filters.First() : builder.Empty)
            .Skip(skip)
            .Limit(take)
            .ToListAsync();
    }
}