
using Microsoft.Extensions.Options;

namespace LTres.Olt.Api.Mongo.Tests.Utils;

public static class MockCommons
{
    public static readonly MongoConfig MongoConfigDefault = new()
    {
        ConnectionString = "mongodb://testmongodb:27017",
        DatabaseName = "testdatabase"
    };

    public static IOptions<MongoConfig> CreateMongoConfigOptions()
        => Options.Create(MongoConfigDefault);

}