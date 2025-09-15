using LTres.Olt.Api.Common.DbServices;
using LTres.Olt.Api.Common.Models;
using MongoDB.Driver;

namespace LTres.Olt.Api.Mongo;

public class MongoDbOLTHost(IMongoDatabase database) : IDbOLTHost, IDbOLTScript
{
    private readonly IMongoCollection<OLT_Host> OLT_Hosts = database.GetCollection<OLT_Host>("olt_hosts");
    private readonly IMongoCollection<OLT_Script> OLT_Scripts = database.GetCollection<OLT_Script>("olt_scripts");

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

    public async Task<Guid> AddOLTScript(OLT_Script oltScript)
    {
        await OLT_Scripts.InsertOneAsync(oltScript);
        return oltScript.Id;
    }

    public async Task<IEnumerable<OLT_Script>> ListOLTScripts(int take = 1000, int skip = 0, Guid? filterId = null, string[]? filterTag = null)
    {
        var builder = Builders<OLT_Script>.Filter;
        var filters = new List<FilterDefinition<OLT_Script>>();

        //mount filters..
        if (filterId.HasValue)
            filters.Add(builder.Eq(o => o.Id, filterId.Value));
        if (filterTag != null)
            foreach (var tag in filterTag)
                filters.Add(builder.AnyEq(o => o.tags, tag));

        //query data..
        return await OLT_Scripts
            .Find(filters.Count > 1 ? builder.And(filters) : filters.Count == 1 ? filters.First() : builder.Empty)
            .Skip(skip)
            .Limit(take)
            .ToListAsync();
    }

    public async Task<int> ChangeOLTScript(OLT_Script oltScript)
    {
        var filter = Builders<OLT_Script>.Filter.Eq(o => o.Id, oltScript.Id);
        var result = await OLT_Scripts.ReplaceOneAsync(filter, oltScript);

        return result.IsAcknowledged && result.IsModifiedCountAvailable ? (int)result.ModifiedCount : 0;
    }
}
