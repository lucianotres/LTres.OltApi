using LTres.Olt.Api.Common;
using Moq;

namespace LTres.Olt.Api.Core.Tests;

public class LogCounterPrinterTests
{
    private readonly Mock<ILogCounter> mockLogCounter;
    private readonly LogCounterPrinter logCounterPrinter;

    public LogCounterPrinterTests()
    {
        mockLogCounter = new();
        logCounterPrinter = new(mockLogCounter.Object);
    }

    [Fact]
    public async Task RunPeriodicNotification_ShouldCallPrintOutAndReset()
    {
        var cts = new CancellationTokenSource();
        mockLogCounter.Setup(s => s.PrintOutAndReset()).Callback(cts.Cancel);

        var task = logCounterPrinter.RunPeriodicNotification(cts.Token);
        await task.WaitAsync(TimeSpan.FromSeconds(5));

        mockLogCounter.Verify(s => s.PrintOutAndReset());
    }
}