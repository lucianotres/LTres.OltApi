
using System.Diagnostics;
using System.Net;
using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.DbServices;
using LTres.Olt.Api.Common.Models;
using LTres.Olt.Api.Core.Workers;
using Microsoft.Extensions.Logging;
using Moq;

namespace LTres.Olt.Api.Core.Tests.Workers;

public class WorkControllerTests
{
    private readonly WorkController workController;
    private readonly Mock<ILogger<WorkController>> mockLogger = new();
    private readonly Mock<ILogCounter> mockLogCounter = new();
    private readonly Mock<IWorkListController> mockWorkListController = new();
    private readonly Mock<IWorkerDispatcher> mockWorkExecutionDispatcher = new();
    private readonly Mock<IWorkerResponseReceiver> mockWorkerResponseReceiver = new();
    private readonly Mock<IWorkResponseController> mockWorkResponseController = new();
    private readonly Mock<IDbWorkCleanUp> mockWorkCleanUp = new();

    public WorkControllerTests()
    {
        workController = new WorkController(
            mockLogger.Object,
            mockLogCounter.Object,
            mockWorkListController.Object,
            mockWorkExecutionDispatcher.Object,
            mockWorkerResponseReceiver.Object,
            mockWorkResponseController.Object,
            mockWorkCleanUp.Object);
    }

    [Fact]
    public void ShouldBeCreatable()
    {
        Assert.NotNull(workController);
    }

    [Fact]
    public async Task Start_ShouldDispatchWorkToDo()
    {
        var workItems = new List<WorkProbeInfo> {
            new() {
                Id = Guid.NewGuid(),
                Host = new IPEndPoint(IPAddress.Loopback, 0),
                Action = "ping"
            },
            new() {
                Id = Guid.NewGuid(),
                Host = new IPEndPoint(IPAddress.Loopback, 0),
                Action = "snmpget"
            }
        };
        List<WorkProbeInfo> workDispatched = [];

        mockWorkListController
            .Setup(s => s.ToBeDone())
            .ReturnsAsync(workItems)
            .Callback(async () => await workController.StopAsync(default));
        mockWorkExecutionDispatcher
            .Setup(d => d.Dispatch(It.IsAny<WorkProbeInfo>()))
            .Callback<WorkProbeInfo>(a => workDispatched.Add(a));

        var stopWatch = Stopwatch.StartNew();
        var maxExecutionTime = TimeSpan.FromSeconds(10);

        await workController.StartAsync(default);
        while (workController.IsRunning && stopWatch.Elapsed < maxExecutionTime)
            await Task.Delay(10);

        stopWatch.Stop();

        Assert.True(stopWatch.Elapsed < maxExecutionTime);
        mockWorkListController.Verify(s => s.ToBeDone(), Times.Once());
        mockWorkExecutionDispatcher.Verify(d => d.Dispatch(It.IsAny<WorkProbeInfo>()), Times.Exactly(2));
        Assert.Equal(2, workDispatched.Count);
    }

    [Fact]
    public async Task Start_ShouldExecuteCleanupPeriodically()
    {
        mockWorkCleanUp
            .Setup(s => s.CleanUpExecute())
            .Callback(async () => await workController.StopAsync(default));

        var stopWatch = Stopwatch.StartNew();
        var maxExecutionTime = TimeSpan.FromSeconds(10);
        workController.CleanUpInterval = 1;

        await workController.StartAsync(default);
        while (workController.IsRunning && stopWatch.Elapsed < maxExecutionTime)
            await Task.Delay(10);

        stopWatch.Stop();

        mockWorkCleanUp.Verify(s => s.CleanUpExecute(), Times.Once());
    }

    [Fact]
    public async Task OnResponseReceived_ShouldSaveResponse()
    {
        var workProbeResponse = new WorkProbeResponse() { Id = Guid.NewGuid() };
        WorkProbeResponse? workProbeResponseToSave = null;
        mockWorkResponseController
            .Setup(w => w.ResponseReceived(It.IsAny<WorkProbeResponse>()))
            .Callback<WorkProbeResponse>(a => workProbeResponseToSave = a);

        mockWorkerResponseReceiver.Raise(r => r.OnResponseReceived += null, new WorkerResponseReceivedEventArgs { ProbeResponse = workProbeResponse });
        await Task.Delay(10);

        mockWorkResponseController.Verify(
            c => c.ResponseReceived(It.Is<WorkProbeResponse>(r => r.Id == workProbeResponse.Id)), Times.Once());
        Assert.NotNull(workProbeResponseToSave);
        Assert.Equal(workProbeResponseToSave, workProbeResponse);
    }
}