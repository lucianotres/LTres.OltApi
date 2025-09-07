using System.Data.Common;
using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LTres.Olt.Api.Mongo;

public class MongoDbOLTHostItem : IDbOLTHostItem
{
    private readonly IMongoCollection<OLT_Host_Item> OLT_Host_Items;
    private readonly IMongoCollection<OLT_Host> OLT_Hosts;

    public MongoDbOLTHostItem(IOptions<MongoConfig> options)
    {
        var config = options.Value;
        var client = new MongoClient(config.ConnectionString);
        var database = client.GetDatabase(config.DatabaseName);

        OLT_Host_Items = database.GetCollection<OLT_Host_Item>("olt_host_items");
        OLT_Hosts = database.GetCollection<OLT_Host>("olt_hosts");
    }

    public async Task<Guid> AddOLTHostItem(OLT_Host_Item item)
    {
        await OLT_Host_Items.InsertOneAsync(item);
        return item.Id;
    }

    public async Task<int> ChangeOLTHostItem(OLT_Host_Item item)
    {
        var filter = Builders<OLT_Host_Item>.Filter.Eq(o => o.Id, item.Id);
        var result = await OLT_Host_Items.ReplaceOneAsync(filter, item);

        return result.IsAcknowledged && result.IsModifiedCountAvailable ? (int)result.ModifiedCount : 0;
    }

    public async Task<IEnumerable<OLT_Host_Item>> ListOLTHostItems(int take = 1000, int skip = 0,
        Guid? filterByOlt = null,
        Guid? filterById = null,
        bool? filterActive = null,
        bool? filterTemplate = null,
        Guid? filterRelated = null,
        string? filterKey = null)
    {
        var builder = Builders<OLT_Host_Item>.Filter;
        var filters = new List<FilterDefinition<OLT_Host_Item>>();

        //mount filters..
        if (filterByOlt.HasValue)
            filters.Add(builder.Eq(o => o.IdOltHost, filterByOlt.Value));
        if (filterById != null)
            filters.Add(builder.Eq(o => o.Id, filterById.Value));

        if (filterRelated == Guid.Empty)
            filters.Add(builder.Eq(o => o.IdRelated, null));
        else if (filterRelated.HasValue)
            filters.Add(builder.Eq(o => o.IdRelated, filterRelated.Value));

        if (filterKey != null)
            filters.Add(builder.Regex(o => o.ItemKey, filterKey));
        if (filterActive.HasValue)
            filters.Add(builder.Eq(o => o.Active, filterActive.Value));
        if (filterTemplate.HasValue)
            filters.Add(builder.Eq(o => o.Template, filterTemplate.Value));

        //query data..
        return await OLT_Host_Items
            .Find(filters.Count > 1 ? builder.And(filters) : filters.Count == 1 ? filters.First() : builder.Empty)
            .Skip(skip)
            .Limit(take)
            .ToListAsync();
    }

    public async Task<OLT_Host_OnuRef?> GetOLTOnuRef(Guid idOlt) =>
        await OLT_Hosts
        .Find(f => f.Id == idOlt)
        .Project(p => p.OnuRef)
        .FirstOrDefaultAsync();

}
