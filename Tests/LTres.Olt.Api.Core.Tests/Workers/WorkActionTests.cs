using System.Net;
using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.Models;
using LTres.Olt.Api.Core.Tools;
using LTres.Olt.Api.Core.Workers;
using Microsoft.Extensions.Logging;
using Moq;

namespace LTres.Olt.Api.Core.Tests.Workers;

public class WorkActionTests
{
    private readonly Mock<ILogger<WorkAction>> mockLogger = new();
    private readonly Mock<IServiceProvider> mockServices = new();
    private readonly Mock<ILogCounter> mockLogCounter = new();
    private readonly Mock<IWorkerActionPing> mockActionPing = new();
    private readonly Mock<IWorkerActionSnmpGet> mockActionSnmpGet = new();
    private readonly Mock<IWorkerActionSnmpWalk> mockActionSnmpWalk = new();
    private readonly Mock<MockPingAction> mockMockActionPing = new();
    private readonly Mock<MockSNMPItems> mockMockSNMPItems; 
    private readonly Mock<MockSnmpGetAction> mockMockSnmpGetAction;
    private readonly Mock<MockSnmpWalkAction> mockMockSnmpWalkAction;

    private readonly WorkAction workAction;

    public WorkActionTests()
    {
        mockMockSNMPItems = new Mock<MockSNMPItems>(MockBehavior.Default, null);
        mockMockSnmpGetAction = new(mockMockSNMPItems.Object);
        mockMockSnmpWalkAction = new(mockMockSNMPItems.Object);

        workAction = new WorkAction(
            mockLogger.Object,
            mockServices.Object,
            mockLogCounter.Object);
    }

    [Fact]
    public void ShouldBeCreatable()
    {
        Assert.NotNull(workAction);
    }

    [Theory]
    [InlineData("ping", false)]
    [InlineData("snmpget", false)]
    [InlineData("snmpwalk", false)]
    [InlineData("unknown", false)]
    [InlineData("ping", true)]
    [InlineData("snmpget", true)]
    [InlineData("snmpwalk", true)]
    [InlineData("unknown", true)]
    public async Task Execute_ShouldPerformACorrectActionByName(string name, bool useMockAction)
    {
        var workProbeInfo = new WorkProbeInfo()
        {
            Id = Guid.NewGuid(),
            Host = new IPEndPoint(useMockAction ? IPAddress.None : IPAddress.Loopback, 0),
            Action = name
        };

        if (useMockAction)
        {
            mockServices.Setup(s => s.GetService(typeof(MockPingAction))).Returns(mockMockActionPing.Object);
            mockServices.Setup(s => s.GetService(typeof(MockSnmpGetAction))).Returns(mockMockSnmpGetAction.Object);
            mockServices.Setup(s => s.GetService(typeof(MockSnmpWalkAction))).Returns(mockMockSnmpWalkAction.Object);
            workProbeInfo.ItemKey = ".1.3.6.1.4.1.3902.1012.3.28.1.1.2.268501248.1";
        }
        else
        {
            mockServices.Setup(s => s.GetService(typeof(IWorkerActionPing))).Returns(mockActionPing.Object);
            mockServices.Setup(s => s.GetService(typeof(IWorkerActionSnmpGet))).Returns(mockActionSnmpGet.Object);
            mockServices.Setup(s => s.GetService(typeof(IWorkerActionSnmpWalk))).Returns(mockActionSnmpWalk.Object);

            mockActionPing
                .Setup(p => p.Execute(It.IsAny<WorkProbeInfo>(), It.IsAny<CancellationToken>(), It.IsAny<WorkProbeResponse>()))
                .ReturnsAsync(new WorkProbeResponse() { Id = workProbeInfo.Id, Success = true, Request = workProbeInfo });
            mockActionSnmpGet
                .Setup(p => p.Execute(It.IsAny<WorkProbeInfo>(), It.IsAny<CancellationToken>(), It.IsAny<WorkProbeResponse>()))
                .ReturnsAsync(new WorkProbeResponse() { Id = workProbeInfo.Id, Success = true, Request = workProbeInfo });
            mockActionSnmpWalk
                .Setup(p => p.Execute(It.IsAny<WorkProbeInfo>(), It.IsAny<CancellationToken>(), It.IsAny<WorkProbeResponse>()))
                .ReturnsAsync(new WorkProbeResponse() { Id = workProbeInfo.Id, Success = true, Request = workProbeInfo });
        }

        var result = await workAction.Execute(workProbeInfo, default);

        if (name == "unknown")
        {
            Assert.False(result.Success);
            Assert.Contains("not found", result.FailMessage);
        }
        else
        {
            Assert.True(result.Success);
            Assert.Equal(workProbeInfo.Id, result.Id);
            Assert.Equal(workProbeInfo, result.Request);
        }
    }
    
}