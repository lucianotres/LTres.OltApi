using LTres.Olt.Api.Common.Models;
using LTres.Olt.Api.Core.Workers;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LTres.Olt.Api.Core.Tests.Workers;

public class WorkPingActionTests
{
    [Fact]
    public async Task Execute_ShouldReturnSuccessfulResponse_WhenPingSucceeds()
    {
        var id = Guid.NewGuid();
        var workPingAction = new WorkPingAction();
        var probeInfo = new WorkProbeInfo { Id = id, Host = new IPEndPoint(IPAddress.Loopback, 0) };
        var cancellationToken = CancellationToken.None;

        var response = await workPingAction.Execute(probeInfo, cancellationToken);

        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.True(response.Success);
        Assert.Equal(WorkProbeResponseType.Value, response.Type);
        Assert.NotNull(response.ValueInt);
        Assert.True(response.ValueInt >= 0);
    }

    [Fact]
    public async Task Execute_ShouldModifyInitialResponse_WhenInitialResponseIsProvided()
    {
        var id = Guid.NewGuid();
        var mockPingAction = new WorkPingAction();
        var probeInfo = new WorkProbeInfo { Id = id, Host = new IPEndPoint(IPAddress.Loopback, 0) };
        var initialResponse = new WorkProbeResponse { Id = id, ValueInt = 0, Success = false };
        var cancellationToken = CancellationToken.None;

        var response = await mockPingAction.Execute(probeInfo, cancellationToken, initialResponse);

        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.True(response.Success);
        Assert.Equal(WorkProbeResponseType.Value, response.Type);
        Assert.NotNull(response.ValueInt);
        Assert.True(response.ValueInt >= 0);        
    }

    [Fact]
    public async Task Execute_ShouldRespectCancellationToken()
    {
        var id = Guid.NewGuid();
        var mockPingAction = new WorkPingAction();
        var probeInfo = new WorkProbeInfo { Id = id, Host = new IPEndPoint(IPAddress.Loopback, 0) };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var response = await mockPingAction.Execute(probeInfo, cts.Token);

        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.False(response.Success);
    }
}
