using LTres.Olt.Api.Mongo.Tests.Utils;

namespace LTres.Olt.Api.Mongo.Tests;

public class MongoDbWorkProbeInfoTests
{
    [Fact]
    public void ShouldBeAbleToCreateIt()
    {
        var options = MockCommons.CreateMongoConfigOptions();

        var dbContext = new MongoDbWorkProbeInfo(options);

        Assert.NotNull(dbContext);
    }
}
