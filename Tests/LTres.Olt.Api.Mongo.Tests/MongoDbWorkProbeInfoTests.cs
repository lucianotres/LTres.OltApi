using System.Net;
using LTres.Olt.Api.Common.Models;
using LTres.Olt.Api.Mongo.Tests.Utils;
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
        mockCursorToProbe.SetupIAsyncCursor(fakeToProbeList);
        mockCollectionToProbe.SetupCollection(mockCursorToProbe);
        mockDatabase.SetupDatabase("to_probe", mockCollectionToProbe);
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
