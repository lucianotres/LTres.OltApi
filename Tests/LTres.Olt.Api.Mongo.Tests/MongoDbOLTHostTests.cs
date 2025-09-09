
using System.Threading.Tasks;
using LTres.Olt.Api.Common.Models;
using LTres.Olt.Api.Mongo.Tests.Utils;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

namespace LTres.Olt.Api.Mongo.Tests;

public class MongoDbOLTHostTests
{
    private readonly Mock<IMongoDatabase> mockDatabase = new();
    private readonly Mock<IMongoCollection<OLT_Host>> mockCollectionOltHosts = new();
    private readonly Mock<IAsyncCursor<OLT_Host>> mockCursorOltHosts = new();
    private readonly Mock<IMongoCollection<OLT_Script>> mockCollectionOltScripts = new();
    private readonly Mock<IAsyncCursor<OLT_Script>> mockCursorOltScripts = new();

    private readonly List<OLT_Host> fakeOltHostsList = [
        new OLT_Host()
        {
            Id = Guid.Empty,
            Name = "Test",
            Host = "1.1.1.1:131"
        }
    ];

    private readonly List<OLT_Script> fakeOltScriptsList = [
        new OLT_Script()
        {
            Id = Guid.Empty
        }
    ];

    public MongoDbOLTHostTests()
    {
        mockCursorOltHosts.SetupIAsyncCursor(fakeOltHostsList);
        mockCollectionOltHosts.SetupCollection(mockCursorOltHosts);
        mockDatabase.SetupDatabase("olt_hosts", mockCollectionOltHosts);

        mockCursorOltScripts.SetupIAsyncCursor(fakeOltScriptsList);
        mockCollectionOltScripts.SetupCollection(mockCursorOltScripts);
        mockDatabase.SetupDatabase("olt_scripts", mockCollectionOltScripts);
    }

    [Fact]
    public void ShouldBeAbleToCreateIt()
    {
        var dbContext = new MongoDbOLTHost(mockDatabase.Object);

        Assert.NotNull(dbContext);
    }

    [Fact]
    public async Task AddOLTHost_ShouldInsertNewHostToDB()
    {
        var dbContext = new MongoDbOLTHost(mockDatabase.Object);
        var newHost = new OLT_Host() { Id = Guid.NewGuid(), Name = "New test host", Host = "1.1.1.1:131" };

        mockCollectionOltHosts
            .Setup(m => m.InsertOneAsync(It.IsAny<OLT_Host>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await dbContext.AddOLTHost(newHost);

        mockCollectionOltHosts.Verify(m => m.InsertOneAsync(newHost, null, default), Times.Once);
        Assert.Equal(newHost.Id, result);
    }

    [Fact]
    public async Task ChangeOLTHost_ShouldCallReplaceOnDB()
    {
        var dbContext = new MongoDbOLTHost(mockDatabase.Object);
        var newHost = new OLT_Host() { Id = Guid.NewGuid(), Name = "New test host", Host = "1.1.1.1:131" };

        mockCollectionOltHosts
            .Setup(m => m.ReplaceOneAsync(It.IsAny<FilterDefinition<OLT_Host>>(), It.IsAny<OLT_Host>(), It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<ReplaceOneResult>(new ReplaceOneResult.Acknowledged(1, 1, BsonObjectId.Empty)));

        var result = await dbContext.ChangeOLTHost(newHost);

        Assert.Equal(1, result);
        mockCollectionOltHosts.Verify(m => m.ReplaceOneAsync(It.IsAny<FilterDefinition<OLT_Host>>(), newHost, null as ReplaceOptions, default), Times.Once);
    }
}