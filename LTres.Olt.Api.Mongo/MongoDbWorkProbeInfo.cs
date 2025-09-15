using LTres.Olt.Api.Common.DbServices;
using LTres.Olt.Api.Common.Models;
using MongoDB.Driver;

namespace LTres.Olt.Api.Mongo;

public class MongoDbWorkProbeInfo(IMongoDatabase database) : IDbWorkProbeInfo
{
    private readonly IMongoCollection<WorkProbeInfo> to_probe = database.GetCollection<WorkProbeInfo>("to_probe");

    public async Task<IEnumerable<WorkProbeInfo>> ToDoList() =>
        await (await to_probe.FindAsync(_ => true)).ToListAsync();
}
