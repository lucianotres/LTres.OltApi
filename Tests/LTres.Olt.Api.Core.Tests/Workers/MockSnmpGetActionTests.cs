using LTres.Olt.Api.Common.Models;
using LTres.Olt.Api.Core.Tools;
using LTres.Olt.Api.Core.Workers;
using System.Net;

namespace LTres.Olt.Api.Core.Tests.Workers;

public class MockSnmpGetActionTests
{
    private static List<int> _RandomValues = [];

    [Fact]
    public async Task Execute_ShouldReturnNonSuccessResponse_WhenOidItemDoesNotExists()
    {
        var id = Guid.NewGuid();
        var mockSnmpGetAction = new MockSnmpGetAction(new MockSNMPItems(null));
        var probeInfo = new WorkProbeInfo
        {
            Id = id,
            Host = new IPEndPoint(IPAddress.Loopback, 0),
            ItemKey = "1"
        };
        var cancellationToken = CancellationToken.None;
        
        var response = await mockSnmpGetAction.Execute(probeInfo, cancellationToken);
        
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal(WorkProbeResponseType.Value, response.Type);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Execute_ShouldReturnSuccessfulResponse_WhenOidItemExists()
    {
        var id = Guid.NewGuid();
        var mockSnmpGetAction = new MockSnmpGetAction(new MockSNMPItems(null));
        var probeInfo = new WorkProbeInfo
        {
            Id = id,
            Host = new IPEndPoint(IPAddress.Loopback, 0),
            ItemKey = "1.3.6.1.4.1.3902.1012.3.28.2.1.4.268501248.1"
        };
        var cancellationToken = CancellationToken.None;
        
        var response = await mockSnmpGetAction.Execute(probeInfo, cancellationToken);
        
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal(WorkProbeResponseType.Value, response.Type);
        Assert.True(response.Success);
        Assert.NotNull(response.ValueInt);
        Assert.Contains(response.ValueInt.Value, [1, 3, 4]);
    }

    public static readonly IEnumerable<object[]> RandomValueCallTimes = [[1], [2], [3], [4], [5], [6], [7], [8], [9]];
    
    [Theory]
    [MemberData(nameof(RandomValueCallTimes))]
    public async Task Execute_ShouldReturnSuccessfulRandomValue_WhenOidItemCalledMoreTimes(int time)
    {
        var id = Guid.NewGuid();
        var mockSnmpGetAction = new MockSnmpGetAction(new MockSNMPItems(null));
        var probeInfo = new WorkProbeInfo
        {
            Id = id,
            Host = new IPEndPoint(IPAddress.Loopback, 0),
            ItemKey = $"1.3.6.1.4.1.3902.1012.3.50.12.1.1.10.268501248.{time}.1"
        };
        var cancellationToken = CancellationToken.None;
        
        var response = await mockSnmpGetAction.Execute(probeInfo, cancellationToken);
        
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal(WorkProbeResponseType.Value, response.Type);
        Assert.True(response.Success);
        Assert.NotNull(response.ValueInt);
        Assert.Contains(response.ValueInt.Value, [3217, 2074, 2725, 2159, 2235, 2657]);

        int value = response.ValueInt.Value;
        _RandomValues.Add(value);

        if (_RandomValues.Count >= 10)
        {
            //10 random values should not be equal
            Assert.False(_RandomValues.All(x => x == value));
            _RandomValues.Clear();
        }
    }

}
