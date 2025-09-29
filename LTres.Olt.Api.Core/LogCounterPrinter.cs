using LTres.Olt.Api.Common;
using Microsoft.Extensions.Hosting;

namespace LTres.Olt.Api.Core;

public class LogCounterPrinter(ILogCounter counter) : IHostedService
{
    private readonly ILogCounter logCounter = counter;
    private CancellationTokenSource? cancellationTokenRunnerPeriodic;
    private const int periodicPrintOutSeconds = 60;
    private const int firstPrintOutSeconds = 0;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationTokenRunnerPeriodic = new CancellationTokenSource();
        _ = RunPeriodicNotification(cancellationTokenRunnerPeriodic.Token);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationTokenRunnerPeriodic?.Cancel();
        return Task.CompletedTask;
    }

    internal Task RunPeriodicNotification(CancellationToken cancellationToken) => Task.Run(async () =>
        {
            int countdownToPrintOut = firstPrintOutSeconds;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (countdownToPrintOut <= 0)
                {
                    countdownToPrintOut = periodicPrintOutSeconds;
                    var strToPrintOut = logCounter.PrintOutAndReset();

                    if (strToPrintOut != null)
                        Console.WriteLine(strToPrintOut);
                }
                else
                    countdownToPrintOut--;

                await Task.Delay(1000);
            }
        }, cancellationToken);

}