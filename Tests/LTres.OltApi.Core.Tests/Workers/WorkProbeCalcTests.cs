using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;
using System.Net;

namespace LTres.OltApi.Core.Tests.Workers;

public abstract class WorkProbeCalcTests<T> where T : IWorkProbeCalc, new()
{

    private async Task<WorkProbeResponse> PrepareAndExecute(int? intVal, uint? uintVal, string? calc)
    {
        var workProbeCalcValues = new T();
        var workProbeInfo = new WorkProbeInfo()
        {
            Host = new IPEndPoint(IPAddress.Loopback, 0),
            Id = Guid.NewGuid(),
            Calc = calc
        };
        var workProbeResponse = new WorkProbeResponse()
        {
            Id = workProbeInfo.Id,
            Type = WorkProbeResponseType.Value,
            Success = true,
            ValueInt = intVal,
            ValueUInt = uintVal
        };

        await workProbeCalcValues.UpdateProbedValuesWithCalculated(workProbeInfo, workProbeResponse);

        return workProbeResponse;
    }


    [Fact]
    public async Task UpdateProbedValuesWithCalculated_ShouldCalculateAdd_WhenIntSimpleValue()
    {
        var workProbeResponse = await PrepareAndExecute(100, null, "val + 10");

        Assert.True(workProbeResponse.Success);
        Assert.NotNull(workProbeResponse.ValueInt);
        Assert.Equal(110, workProbeResponse.ValueInt.Value);
    }

    [Fact]
    public async Task UpdateProbedValuesWithCalculated_ShouldCalculateMultiply_WhenIntSimpleValue()
    {
        var workProbeResponse = await PrepareAndExecute(200, null, "val * 0.1");

        Assert.True(workProbeResponse.Success);
        Assert.NotNull(workProbeResponse.ValueInt);
        Assert.Equal(20, workProbeResponse.ValueInt.Value);
    }

    [Fact]
    public async Task UpdateProbedValuesWithCalculated_ShouldNotCalculate_WhenIntSimpleValueWithoutCalc()
    {
        var workProbeResponse = await PrepareAndExecute(200, null, null);

        Assert.True(workProbeResponse.Success);
        Assert.NotNull(workProbeResponse.ValueInt);
        Assert.Equal(200, workProbeResponse.ValueInt.Value);
    }


    [Fact]
    public async Task UpdateProbedValuesWithCalculated_ShouldCalculateAdd_WhenUIntSimpleValue()
    {
        var workProbeResponse = await PrepareAndExecute(null, 100, "val + 10");

        Assert.True(workProbeResponse.Success);
        Assert.NotNull(workProbeResponse.ValueUInt);
        Assert.Equal((uint)110, workProbeResponse.ValueUInt.Value);
    }

    [Fact]
    public async Task UpdateProbedValuesWithCalculated_ShouldCalculateMultiply_WhenUIntSimpleValue()
    {
        var workProbeResponse = await PrepareAndExecute(null, 200, "val * 0.1");

        Assert.True(workProbeResponse.Success);
        Assert.NotNull(workProbeResponse.ValueUInt);
        Assert.Equal((uint)20, workProbeResponse.ValueUInt.Value);
    }

    [Fact]
    public async Task UpdateProbedValuesWithCalculated_ShouldNotCalculate_WhenUIntSimpleValueWithoutCalc()
    {
        var workProbeResponse = await PrepareAndExecute(null, 200, null);

        Assert.True(workProbeResponse.Success);
        Assert.NotNull(workProbeResponse.ValueUInt);
        Assert.Equal((uint)200, workProbeResponse.ValueUInt.Value);
    }

    private async Task<WorkProbeResponse> PrepareAndExecuteWalk(int? intVal, uint? uintVal, string? calc)
    {
        var workProbeCalcValues = new WorkProbeCalcValues();
        var workProbeInfo = new WorkProbeInfo()
        {
            Host = new IPEndPoint(IPAddress.Loopback, 0),
            Id = Guid.NewGuid(),
            Calc = calc
        };
        var workProbeResponse = new WorkProbeResponse()
        {
            Id = workProbeInfo.Id,
            Type = WorkProbeResponseType.Walk,
            Success = true,
            Values = new List<WorkProbeResponseVar>()
            {
                new ()
                {
                    ValueInt = intVal,
                    ValueUInt = uintVal
                },
                new ()
                {
                    ValueInt = intVal,
                    ValueUInt = uintVal
                }
            }
        };

        await workProbeCalcValues.UpdateProbedValuesWithCalculated(workProbeInfo, workProbeResponse);

        return workProbeResponse;
    }

    [Fact]
    public async Task UpdateProbedValuesWithCalculated_ShouldCalculateAdd_WhenIntWalkValue()
    {
        var workProbeResponse = await PrepareAndExecuteWalk(100, null, "val + 10");

        Assert.True(workProbeResponse.Success);
        Assert.NotNull(workProbeResponse.Values);
        foreach (var v in workProbeResponse.Values)
        {
            Assert.NotNull(v.ValueInt);
            Assert.Equal(110, v.ValueInt.Value);
        }
    }

    [Fact]
    public async Task UpdateProbedValuesWithCalculated_ShouldCalculateMultiply_WhenIntWalkValue()
    {
        var workProbeResponse = await PrepareAndExecuteWalk(200, null, "val * 0.1");

        Assert.True(workProbeResponse.Success);
        Assert.NotNull(workProbeResponse.Values);
        foreach (var v in workProbeResponse.Values)
        {
            Assert.NotNull(v.ValueInt);
            Assert.Equal(20, v.ValueInt.Value);
        }
    }

    [Fact]
    public async Task UpdateProbedValuesWithCalculated_ShouldNotCalculate_WhenIntWalkValueWithoutCalc()
    {
        var workProbeResponse = await PrepareAndExecuteWalk(200, null, null);

        Assert.True(workProbeResponse.Success);
        Assert.NotNull(workProbeResponse.Values);
        foreach (var v in workProbeResponse.Values)
        {
            Assert.NotNull(v.ValueInt);
            Assert.Equal(200, v.ValueInt.Value);
        }
    }


    [Fact]
    public async Task UpdateProbedValuesWithCalculated_ShouldCalculateAdd_WhenUIntWalkValue()
    {
        var workProbeResponse = await PrepareAndExecuteWalk(null, 100, "val + 10");

        Assert.True(workProbeResponse.Success);
        Assert.NotNull(workProbeResponse.Values);
        foreach (var v in workProbeResponse.Values)
        {
            Assert.NotNull(v.ValueUInt);
            Assert.Equal((uint)110, v.ValueUInt.Value);
        }
    }

    [Fact]
    public async Task UpdateProbedValuesWithCalculated_ShouldCalculateMultiply_WhenUIntWalkValue()
    {
        var workProbeResponse = await PrepareAndExecuteWalk(null, 200, "val * 0.1");

        Assert.True(workProbeResponse.Success);
        Assert.NotNull(workProbeResponse.Values);
        foreach (var v in workProbeResponse.Values)
        {
            Assert.NotNull(v.ValueUInt);
            Assert.Equal((uint)20, v.ValueUInt.Value);
        }
    }

    [Fact]
    public async Task UpdateProbedValuesWithCalculated_ShouldNotCalculate_WhenUIntWalkValueWithoutCalc()
    {
        var workProbeResponse = await PrepareAndExecuteWalk(null, 200, null);

        Assert.True(workProbeResponse.Success);
        Assert.NotNull(workProbeResponse.Values);
        foreach (var v in workProbeResponse.Values)
        {
            Assert.NotNull(v.ValueUInt);
            Assert.Equal((uint)200, v.ValueUInt.Value);
        }
    }

}
