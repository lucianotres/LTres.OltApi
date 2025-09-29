using LTres.Olt.Api.Common;

namespace LTres.Olt.Api.Core.Tests;

public class LogCounterTests
{
    private readonly LogCounter logCounter = new();

    [Fact]
    public void AddSuccess_ShouldRegisterNewLog()
    {
        var logGuid = Guid.NewGuid();

        logCounter.AddSuccess(logGuid, "test");
        var registeredLogs = logCounter.GetLogCounters();

        Assert.Single(registeredLogs);
        var log = registeredLogs.First();
        Assert.Equal(logGuid, log.Id);
        Assert.Equal("test", log.Category);
        Assert.True(log.Success.GetValueOrDefault());
    }

    [Fact]
    public void AddError_ShouldRegisterNewLog()
    {
        var logGuid = Guid.NewGuid();
        var logException = new Exception("Generic");

        logCounter.AddError(logGuid, "test", null, logException);
        var registeredLogs = logCounter.GetLogCounters();

        Assert.Single(registeredLogs);
        var log = registeredLogs.First();
        Assert.Equal(logGuid, log.Id);
        Assert.Equal("test", log.Category);
        Assert.True(log.Success.HasValue);
        Assert.False(log.Success.Value);
        Assert.Equal(logException, log.Error);
    }

    [Fact]
    public void AddCount_ShouldRegisterNewLog()
    {
        logCounter.AddCount("test", 3);
        var registeredLogs = logCounter.GetLogCounters();

        Assert.Single(registeredLogs);
        var log = registeredLogs.First();
        Assert.Equal("test", log.Category);
        Assert.Equal(3, log.Quantity);
    }

    [Fact]
    public void PrintOutAndReset_ShouldReturnAStringWithFormattedLog()
    {
        logCounter.AddCount("test category", 3);

        var strLog = logCounter.PrintOutAndReset();

        Assert.Contains("test category", strLog);
    }

    [Fact]
    public void RegisterHookOnPrintResetAction_ShouldRegisterNewHook_And_ShouldBeCalledOnReset()
    {
        bool resetCalled = false;
        void hookResetAction(ILogCounter log) => resetCalled = true;
        logCounter.RegisterHookOnPrintResetAction<LogCounterTests>(hookResetAction);

        logCounter.PrintOutAndReset();

        Assert.True(resetCalled);
    }
}