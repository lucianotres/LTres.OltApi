using LTres.OltApi.Common.Models;
using LTres.OltApi.Core.Workers;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LTres.OltApi.Core.Tests.Workers;

public class MockPingActionTests
{
    [Fact]
    public async Task Execute_ShouldReturnSuccessfulWithRandomLatencyValue()
    {
        var id = Guid.NewGuid();
        var mockPingAction = new MockPingAction();
        var probeInfo = new WorkProbeInfo { Id = id, Host = new IPEndPoint(IPAddress.Loopback, 0) };
        var cancellationToken = CancellationToken.None;

        var response = await mockPingAction.Execute(probeInfo, cancellationToken);

        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal(WorkProbeResponseType.Value, response.Type);
        Assert.NotNull(response.ValueInt);
        Assert.InRange(response.ValueInt.Value, 10, 90);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task Execute_ShouldModifyInitialResponse_WhenInitialResponseIsProvided()
    {
        var id = Guid.NewGuid();
        var mockPingAction = new MockPingAction();
        var probeInfo = new WorkProbeInfo { Id = id, Host = new IPEndPoint(IPAddress.Loopback, 0) };
        var initialResponse = new WorkProbeResponse { Id = id, ValueInt = 0, Success = false };
        var cancellationToken = CancellationToken.None;

        var response = await mockPingAction.Execute(probeInfo, cancellationToken, initialResponse);

        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal(WorkProbeResponseType.Value, response.Type);
        Assert.NotNull(response.ValueInt);
        Assert.InRange(response.ValueInt.Value, 10, 90);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task Execute_ShouldRespectCancellationToken()
    {
        var id = Guid.NewGuid();
        var mockPingAction = new MockPingAction();
        var probeInfo = new WorkProbeInfo { Id = id, Host = new IPEndPoint(IPAddress.Loopback, 0) };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var response = await mockPingAction.Execute(probeInfo, cts.Token);

        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.False(response.Success);
    }
}
