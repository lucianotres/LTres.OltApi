using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.DbServices;
using LTres.Olt.Api.Common.Models;
using LTres.Olt.Api.Core.Workers;
using Moq;

namespace LTres.Olt.Api.Core.Tests.Workers;

public class WorkDoneManagerTests
{
    private readonly Mock<IWorkProbeCache> mockWorkProbeCache = new();
    private readonly Mock<IDbWorkProbeResponse> mockDbWorkProbeResponse = new();
    private readonly WorkDoneManager workDoneManager;

    public WorkDoneManagerTests()
    {
        workDoneManager = new WorkDoneManager(
            mockWorkProbeCache.Object,
            mockDbWorkProbeResponse.Object);
    }

    [Fact]
    public void ShouldBeCreatable()
    {
        Assert.NotNull(workDoneManager);
    }

    [Fact]
    public async Task ResponseReceived_ShouldRemoveFromCache()
    {
        var workProbeResponse = new WorkProbeResponse() { Id = Guid.NewGuid() };

        await workDoneManager.ResponseReceived(workProbeResponse);

        mockWorkProbeCache.Verify(x => x.TryToRemoveFromCache(workProbeResponse.Id), Times.Once());
    }

    [Theory]
    [InlineData(WorkProbeResponseType.Value, true)]
    [InlineData(WorkProbeResponseType.Walk, true)]
    [InlineData(WorkProbeResponseType.Value, false)]
    [InlineData(WorkProbeResponseType.Walk, false)]
    public async Task ResponseReceived_ShouldSaveOnDB(WorkProbeResponseType byType, bool success)
    {
        var workProbeResponse = new WorkProbeResponse()
        {
            Id = Guid.NewGuid(),
            Type = byType,
            Success = success
        };

        if (success && byType == WorkProbeResponseType.Walk)
        {
            List<OLT_Host_Item> templateItems =
            [
                new OLT_Host_Item() { Id = Guid.NewGuid(), ItemKey = "1" },
                new OLT_Host_Item() { Id = Guid.NewGuid(), ItemKey = "2" }
            ];
            mockDbWorkProbeResponse
                .Setup(x => x.GetItemTemplates(It.IsAny<Guid>()))
                .ReturnsAsync(templateItems);
        }

        await workDoneManager.ResponseReceived(workProbeResponse);

        mockDbWorkProbeResponse.Verify(x => x.SaveWorkProbeResponse(workProbeResponse), Times.Once());
        if (success && byType == WorkProbeResponseType.Walk)
        {
            mockDbWorkProbeResponse.Verify(x => x.GetItemTemplates(workProbeResponse.Id), Times.Once());
            mockDbWorkProbeResponse.Verify(x => x.CreateItemsFromTemplate(It.IsAny<OLT_Host_Item>(), workProbeResponse), Times.Exactly(2));
        }
    }
}