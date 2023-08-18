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
        var client = new MongoClient(new MongoClientSettings()
        {
            Server = new MongoServerAddress("localhost"),
            ClusterConfigurator = cb =>
            {
                cb.Subscribe<CommandStartedEvent>(e => Console.WriteLine($"{e.CommandName} - {e.Command.ToJson()}"));
            }
        });

        var database = client.GetDatabase(config.DatabaseName);

        OLT_Hosts = database.GetCollection<OLT_Host>("olt_hosts");
    }

    private IMongoCollection<OLT_Host> OLT_Hosts;

    public async Task<Guid> AddOLTHost(OLT_Host olt)
    {
        await OLT_Hosts.InsertOneAsync(olt);
        return olt.Id;
    }

    public async Task<IEnumerable<OLT_Host>> ListOLTHosts(int take = 1000, int skip = 0,
        Guid? filterId = null,
        string? filterName = null,
        string? filterHost = null,
        string[]? filterTag = null)
    {
        var builder = Builders<OLT_Host>.Filter;
        var filters = new List<FilterDefinition<OLT_Host>>();

        //mount filters..
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