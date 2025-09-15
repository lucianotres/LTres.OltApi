using LTres.Olt.Api.Common.Models;
using LTres.Olt.Api.Mongo.Tests.Utils;
using MongoDB.Driver;
using Moq;

namespace LTres.Olt.Api.Mongo.Tests;

public class MongoDbWorkProbeResponseTests
{
    private readonly Mock<IMongoDatabase> mockDatabase = new();
    private readonly Mock<IMongoCollection<OLT_Host_Item>> mockCollectionHostItem = new();
    private readonly Mock<IAsyncCursor<OLT_Host_Item>> mockCursorHostItem = new();
    private readonly Mock<IMongoCollection<OLT_Host_Item_History>> mockCollectionHostItemHistory = new();
    private readonly Mock<IAsyncCursor<OLT_Host_Item_History>> mockCursorHostItemHistory = new();

    public MongoDbWorkProbeResponseTests()
    {
        mockCursorHostItem.SetupIAsyncCursor([]);
        mockCollectionHostItem.SetupCollection(mockCursorHostItem);
        mockDatabase.SetupDatabase("olt_host_items", mockCollectionHostItem);

        mockCursorHostItemHistory.SetupIAsyncCursor([]);
        mockCollectionHostItemHistory.SetupCollection(mockCursorHostItemHistory);
        mockDatabase.SetupDatabase("olt_host_items_history", mockCollectionHostItemHistory);
    }

    [Fact]
    public void ShouldBeAbleToCreateIt()
    {
        var dbContext = new MongoDbWorkProbeResponse(mockDatabase.Object);

        Assert.NotNull(dbContext);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task SaveWorkProbeResponse_ShouldSave_ItemValue_OnDB(bool withSuccess, bool withHistory)
    {
        var newWorkProbeResponse = new WorkProbeResponse()
        {
            Id = Guid.NewGuid(),
            Success = withSuccess,
            ProbedAt = DateTime.UtcNow,
            Type = WorkProbeResponseType.Value,
            DoHistory = withHistory,
            Values = withSuccess ?
            [
                new WorkProbeResponseVar() { ValueInt = 1 },
                new WorkProbeResponseVar() { ValueInt = 2 },
                new WorkProbeResponseVar() { ValueInt = 3 }
            ] :
            []
        };

        var dbContext = new MongoDbWorkProbeResponse(mockDatabase.Object);

        await dbContext.SaveWorkProbeResponse(newWorkProbeResponse);

        mockCollectionHostItem.Verify(m => m.UpdateOneAsync(
            It.IsAny<FilterDefinition<OLT_Host_Item>>(),
            It.IsAny<UpdateDefinition<OLT_Host_Item>>(),
            null, default), Times.Once);

        mockCollectionHostItemHistory.Verify(m => m.InsertOneAsync(
            It.IsAny<OLT_Host_Item_History>(),
            null, default), withHistory && withSuccess ? Times.Once : Times.Never);
    }
    
}