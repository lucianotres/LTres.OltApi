using LTres.Olt.Api.Common.Models;
using LTres.Olt.Api.Mongo.Tests.Utils;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;

namespace LTres.Olt.Api.Mongo.Tests;

public class MongoDbWorkCleanUpTests
{
    private readonly Mock<ILogger<MongoDbWorkCleanUp>> mockLogger = new();
    private readonly Mock<IMongoDatabase> mockDatabase = new();
    private readonly Mock<IMongoCollection<OLT_Host_Item>> mockCollectionHostItem = new();
    private readonly Mock<IAsyncCursor<OLT_Host_Item>> mockCursorHostItem = new();
    private readonly Mock<IMongoCollection<OLT_Host_Item_History>> mockCollectionHostItemHistory = new();
    private readonly Mock<IAsyncCursor<OLT_Host_Item_History>> mockCursorHostItemHistory = new();
    private readonly Mock<IMongoCollection<MongoDbWorkCleanUp.IdModel>> mockCollectionIdModel = new();
    private readonly Mock<IAsyncCursor<MongoDbWorkCleanUp.IdModel>> mockCursorIdModel = new();
    private readonly Mock<IAsyncCursor<MongoDbWorkCleanUp.CleanUpItemHistory>> mockCursorCleanUpItemHistory = new();
    private readonly Mock<IAsyncCursor<MongoDbWorkCleanUp.CleanUpItemHistory>> mockCursorCleanUpItemHistoryOrphans = new();


    public MongoDbWorkCleanUpTests()
    {
        mockCollectionHostItem.SetupCollection(mockCursorHostItem);
        mockDatabase.SetupDatabase("olt_host_items", mockCollectionHostItem);

        mockCollectionHostItemHistory.SetupCollection(mockCursorHostItemHistory);
        mockDatabase.SetupDatabase("olt_host_items_history", mockCollectionHostItemHistory);

        mockCollectionHostItem
            .Setup(c => c.Aggregate(
                It.IsAny<PipelineDefinition<OLT_Host_Item, MongoDbWorkCleanUp.CleanUpItemHistory>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(mockCursorCleanUpItemHistory.Object);

        mockCollectionHostItemHistory
            .Setup(c => c.Aggregate(
                It.IsAny<PipelineDefinition<OLT_Host_Item_History, MongoDbWorkCleanUp.CleanUpItemHistory>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(mockCursorCleanUpItemHistoryOrphans.Object);

        mockCollectionIdModel.SetupCollection(mockCursorIdModel);
        mockDatabase.SetupDatabase("to_remove_olt_items", mockCollectionIdModel);
    }

    [Fact]
    public void ShouldBeAbleToCreateIt()
    {
        var dbContext = new MongoDbWorkCleanUp(mockDatabase.Object, mockLogger.Object);

        Assert.NotNull(dbContext);
    }

    [Fact]
    public async Task CleanUpExecute_ShouldRemoveExpiredItemsOnDatabase()
    {
        //setup
        mockCursorIdModel.SetupIAsyncCursor([
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() }
        ]);
        mockCollectionHostItem
            .Setup(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<OLT_Host_Item>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteResult.Acknowledged(3));

        mockCursorCleanUpItemHistory.SetupIAsyncCursor([
            new() { Id = Guid.NewGuid(), Until = DateTime.UtcNow.AddDays(-1) },
            new() { Id = Guid.NewGuid(), Until = DateTime.UtcNow.AddDays(-2) },
            new() { Id = Guid.NewGuid(), Until = DateTime.UtcNow.AddDays(-3) }
        ]);

        mockCursorCleanUpItemHistoryOrphans.SetupIAsyncCursor([
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() }
        ]);
        mockCollectionHostItemHistory
            .Setup(c => c.DeleteManyAsync(It.IsAny<FilterDefinition<OLT_Host_Item_History>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteResult.Acknowledged(1));


        //act
        var dbContext = new MongoDbWorkCleanUp(mockDatabase.Object, mockLogger.Object);
        var result = await dbContext.CleanUpExecute();

        //assert
        Assert.Equal(14, result);
        mockCollectionIdModel.Verify(c =>
            c.FindAsync(
                It.IsAny<FilterDefinition<MongoDbWorkCleanUp.IdModel>>(),
                It.IsAny<FindOptions<MongoDbWorkCleanUp.IdModel, MongoDbWorkCleanUp.IdModel>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        mockCollectionHostItem.Verify(c =>
            c.Aggregate(
                It.IsAny<PipelineDefinition<OLT_Host_Item, MongoDbWorkCleanUp.CleanUpItemHistory>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        mockCollectionHostItemHistory.Verify(c =>
            c.Aggregate(
                It.IsAny<PipelineDefinition<OLT_Host_Item_History, MongoDbWorkCleanUp.CleanUpItemHistory>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        mockCollectionHostItemHistory.Verify(c =>
            c.DeleteManyAsync(
                It.IsAny<FilterDefinition<OLT_Host_Item_History>>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(5));
    }
}