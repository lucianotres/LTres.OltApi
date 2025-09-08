using System.Net;
using LTres.Olt.Api.Common.Models;
using MongoDB.Driver;
using Moq;

namespace LTres.Olt.Api.Mongo.Tests;

public class MongoDbWorkProbeInfoTests
{
    private readonly Mock<IMongoDatabase> mockDatabase = new();
    private readonly Mock<IMongoCollection<WorkProbeInfo>> mockCollectionToProbe = new();
    private readonly Mock<IAsyncCursor<WorkProbeInfo>> mockCursorToProbe = new();

    private readonly List<WorkProbeInfo> fakeToProbeList = [
        new WorkProbeInfo()
        {
            Id = Guid.Empty,
            Host = IPEndPoint.Parse("1.1.1.1:131"),
            SnmpCommunity = "public",
            SnmpVersion = 1,
            SnmpBulk = true
        }
    ];


    public MongoDbWorkProbeInfoTests()
    {
        mockCursorToProbe.Setup(x => x.Current).Returns(fakeToProbeList);
        mockCursorToProbe
            .SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursorToProbe
            .SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Returns(Task.FromResult(false));

        mockCollectionToProbe
            .Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<WorkProbeInfo>>(),
                It.IsAny<FindOptions<WorkProbeInfo, WorkProbeInfo>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursorToProbe.Object);

        mockDatabase
            .Setup(m => m.GetCollection<WorkProbeInfo>("to_probe", null))
            .Returns(mockCollectionToProbe.Object);
    }

    [Fact]
    public void ShouldBeAbleToCreateIt()
    {
        var dbContext = new MongoDbWorkProbeInfo(mockDatabase.Object);

        Assert.NotNull(dbContext);
    }

    [Fact]
    public async Task ToDoList_ShouldReturnAListOfWorkProbeInfo()
    {
        var dbContext = new MongoDbWorkProbeInfo(mockDatabase.Object);

        var resultList = await dbContext.ToDoList();

        Console.WriteLine($"Test here: {resultList.Count()}");

        Assert.NotNull(resultList);
        Assert.NotEmpty(resultList);
    }

}
