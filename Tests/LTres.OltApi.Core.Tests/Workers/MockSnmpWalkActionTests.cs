using LTres.OltApi.Common.Models;
using LTres.OltApi.Core.Tools;
using LTres.OltApi.Core.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LTres.OltApi.Core.Tests.Workers;

public class MockSnmpWalkActionTests
{
    [Fact]
    public async Task Execute_ShouldReturnANotNullResponse()
    {
        var id = Guid.NewGuid();
        var mockSnmpWalkAction = new MockSnmpWalkAction(new MockSNMPItems(null));
        var probeInfo = new WorkProbeInfo { Id = id, Host = new (IPAddress.Loopback, 0), ItemKey = "123.456" };
        var cancellationToken = CancellationToken.None;
        
        var response = await mockSnmpWalkAction.Execute(probeInfo, cancellationToken);
        
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
    }


    [Fact]
    public async Task Execute_ShouldReturnAWalkTypeResponse()
    {
        var id = Guid.NewGuid();
        var mockSnmpWalkAction = new MockSnmpWalkAction(new MockSNMPItems(null));
        var probeInfo = new WorkProbeInfo { Id = id, Host = new(IPAddress.Loopback, 0), ItemKey = "123.456" };
        var cancellationToken = CancellationToken.None;

        var response = await mockSnmpWalkAction.Execute(probeInfo, cancellationToken);

        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal(WorkProbeResponseType.Walk, response.Type);
    }

    [Fact]
    public async Task Execute_ShouldReturnFailedResponse_WhenOidNotExists()
    {
        var id = Guid.NewGuid();
        var mockSnmpWalkAction = new MockSnmpWalkAction(new MockSNMPItems(null));
        var probeInfo = new WorkProbeInfo { Id = id, Host = new(IPAddress.Loopback, 0), ItemKey = "123.456" };
        var cancellationToken = CancellationToken.None;

        var response = await mockSnmpWalkAction.Execute(probeInfo, cancellationToken);

        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal(WorkProbeResponseType.Walk, response.Type);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Execute_ShouldReturnSuccessfulResponse_WhenOidExists()
    {
        var id = Guid.NewGuid();
        var mockSnmpWalkAction = new MockSnmpWalkAction(new MockSNMPItems(null));
        var probeInfo = new WorkProbeInfo { Id = id, Host = new(IPAddress.Loopback, 0), ItemKey = "1.3.6.1.4.1.3902.1012.3.28.1.1.2" };
        var cancellationToken = CancellationToken.None;

        var response = await mockSnmpWalkAction.Execute(probeInfo, cancellationToken);

        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal(WorkProbeResponseType.Walk, response.Type);
        Assert.True(response.Success);
        Assert.NotNull(response.Values);
        Assert.NotEmpty(response.Values);
    }

    [Fact]
    public async Task Execute_ShouldReturnMoreThenOneValueResponse_WhenOidWalkSuccessfully()
    {
        var id = Guid.NewGuid();
        var mockSnmpWalkAction = new MockSnmpWalkAction(new MockSNMPItems(null));
        var probeInfo = new WorkProbeInfo { Id = id, Host = new(IPAddress.Loopback, 0), ItemKey = "1.3.6.1.4.1.3902.1012.3.28.1.1.2" };
        var cancellationToken = CancellationToken.None;

        var response = await mockSnmpWalkAction.Execute(probeInfo, cancellationToken);

        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal(WorkProbeResponseType.Walk, response.Type);
        Assert.True(response.Success);
        Assert.NotNull(response.Values);
        Assert.True(response.Values.Count() > 1);
    }

}
