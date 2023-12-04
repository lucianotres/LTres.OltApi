using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Common.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LTres.OltApi.Mongo;

public class MongoDbWorkProbeInfo : IDbWorkProbeInfo
{
    private IMongoCollection<OLT_Host_Item> OLT_Host_Items;
    private IMongoCollection<WorkProbeInfo> to_probe;

    public MongoDbWorkProbeInfo(IOptions<MongoConfig> options)
    {
        var config = options.Value;
        var client = new MongoClient(config.ConnectionString);
        var database = client.GetDatabase(config.DatabaseName);

        OLT_Host_Items = database.GetCollection<OLT_Host_Item>("olt_host_items");
        to_probe = database.GetCollection<WorkProbeInfo>("to_probe");
    }

    public async Task<IEnumerable<WorkProbeInfo>> ToDoList() =>
        await (await to_probe.FindAsync(_ => true)).ToListAsync();
}