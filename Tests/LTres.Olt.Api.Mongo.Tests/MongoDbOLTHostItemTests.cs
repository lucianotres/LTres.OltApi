using LTres.Olt.Api.Common.Models;
using LTres.Olt.Api.Mongo.Tests.Utils;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

namespace LTres.Olt.Api.Mongo.Tests;

public class MongoDbOLTHostItemTests
{
    private readonly Mock<IMongoDatabase> mockDatabase = new();
    private readonly Mock<IMongoCollection<OLT_Host_Item>> mockCollectionHostItem = new();
    private readonly Mock<IAsyncCursor<OLT_Host_Item>> mockCursorHostItem = new();

    private readonly List<OLT_Host_Item> fakeHostItemList = [
        new OLT_Host_Item()
        {
            Id = Guid.Empty,
            IdOltHost = Guid.Empty
        }
    ];

    public MongoDbOLTHostItemTests()
    {
        mockCursorHostItem.SetupIAsyncCursor(fakeHostItemList);
        mockCollectionHostItem.SetupCollection(mockCursorHostItem);
        mockDatabase.SetupDatabase("olt_host_items", mockCollectionHostItem);
    }

    [Fact]
    public void ShouldBeAbleToCreateIt()
    {
        var dbContext = new MongoDbOLTHostItem(mockDatabase.Object);

        Assert.NotNull(dbContext);
    }

    [Fact]
    public async Task AddOLTHostItem_ShouldInsertNewHostItemToDB()
    {
        var dbContext = new MongoDbOLTHostItem(mockDatabase.Object);
        var newHostItem = new OLT_Host_Item() { Id = Guid.NewGuid(), IdOltHost = Guid.NewGuid() };

        mockCollectionHostItem
            .Setup(m => m.InsertOneAsync(It.IsAny<OLT_Host_Item>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await dbContext.AddOLTHostItem(newHostItem);

        mockCollectionHostItem.Verify(m => m.InsertOneAsync(newHostItem, null, default), Times.Once);
        Assert.Equal(newHostItem.Id, result);
    }


    [Fact]
    public async Task ChangeOLTHostItem_ShouldCallReplaceOnDB()
    {
        var dbContext = new MongoDbOLTHostItem(mockDatabase.Object);
        var newHostItem = new OLT_Host_Item() { Id = Guid.NewGuid(), IdOltHost = Guid.NewGuid() };

        mockCollectionHostItem
            .Setup(m => m.ReplaceOneAsync(It.IsAny<FilterDefinition<OLT_Host_Item>>(), It.IsAny<OLT_Host_Item>(), It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<ReplaceOneResult>(new ReplaceOneResult.Acknowledged(1, 1, BsonObjectId.Empty)));

        var result = await dbContext.ChangeOLTHostItem(newHostItem);

        Assert.Equal(1, result);
        mockCollectionHostItem.Verify(m => m.ReplaceOneAsync(It.IsAny<FilterDefinition<OLT_Host_Item>>(), newHostItem, null as ReplaceOptions, default), Times.Once);
    }

    [Fact]
    public async Task ListOLTHosts_ShouldReturnAFullList()
    {
        var dbContext = new MongoDbOLTHostItem(mockDatabase.Object);

        var result = await dbContext.ListOLTHostItems();

        mockCollectionHostItem.Verify(x => x.FindAsync(It.IsAny<FilterDefinition<OLT_Host_Item>>(), It.IsAny<FindOptions<OLT_Host_Item, OLT_Host_Item>>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Single(result);
        Assert.Equal(fakeHostItemList, result);
    }
}