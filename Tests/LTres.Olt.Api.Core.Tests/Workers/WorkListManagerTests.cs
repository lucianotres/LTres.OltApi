using System.Net;
using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.DbServices;
using LTres.Olt.Api.Common.Models;
using LTres.Olt.Api.Core.Workers;
using Moq;

namespace LTres.Olt.Api.Core.Tests.Workers;

public class WorkListManagerTests
{
    private readonly Mock<IDbWorkProbeInfo> mockDbWorkProbeInfo = new();
    private readonly Mock<IWorkProbeCache> mockWorkProbeCache = new();
    private readonly WorkListManager workListManager;

    public WorkListManagerTests()
    {
        workListManager = new WorkListManager(
            mockDbWorkProbeInfo.Object,
            mockWorkProbeCache.Object);
    }

    [Fact]
    public void ShouldBeCreatable()
    {
        Assert.NotNull(workListManager);
    }

    [Fact]
    public async Task ToBeDone_ShouldReturnAListOfWorkToBeDoneThatIsNotInCache()
    {
        List<WorkProbeInfo> workProbeList =
        [
            new WorkProbeInfo() { Id = Guid.NewGuid(), Host = new IPEndPoint(IPAddress.Loopback, 0) },
            new WorkProbeInfo() { Id = Guid.NewGuid(), Host = new IPEndPoint(IPAddress.Loopback, 0) },
            new WorkProbeInfo() { Id = Guid.NewGuid(), Host = new IPEndPoint(IPAddress.Loopback, 0) },
            new WorkProbeInfo() { Id = Guid.NewGuid(), Host = new IPEndPoint(IPAddress.Loopback, 0) },
            new WorkProbeInfo() { Id = Guid.NewGuid(), Host = new IPEndPoint(IPAddress.Loopback, 0) },
        ];

        mockDbWorkProbeInfo
            .Setup(x => x.ToDoList())
            .ReturnsAsync(workProbeList);

        mockWorkProbeCache
            .Setup(x => x.TryToPutIntoCache(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ReturnsAsync((Guid g, DateTime t) => workProbeList[0].Id != g);

        var result = await workListManager.ToBeDone();

        Assert.Equal(4, result.Count());
        Assert.Equal(workProbeList.Skip(1), result);
        mockDbWorkProbeInfo.Verify(x => x.ToDoList(), Times.Once());
        mockWorkProbeCache.Verify(x => x.TryToPutIntoCache(It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Exactly(5));
    }
}