using System.Collections.Concurrent;
using System.Text;
using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace LTres.Olt.Api.Core;

public class LogCounterPrinter : IHostedService
{
    private readonly ILogger log;
    private readonly ILogCounter logCounter;
    private CancellationTokenSource? cancellationTokenRunnerPeriodic;
    private const int periodicPrintOutSeconds = 60;

    public LogCounterPrinter(ILogger<LogCounterPrinter> logger, ILogCounter counter)
    {
        log = logger;
        logCounter = counter;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationTokenRunnerPeriodic = new CancellationTokenSource();
        _ = RunPeriodicNotification(cancellationTokenRunnerPeriodic.Token);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (cancellationTokenRunnerPeriodic != null)
            cancellationTokenRunnerPeriodic.Cancel();

        return Task.CompletedTask;
    }

    private Task RunPeriodicNotification(CancellationToken cancellationToken) => Task.Run(async () =>
        {
            int countdownToPrintOut = 15;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (countdownToPrintOut <= 0)
                {
                    countdownToPrintOut = periodicPrintOutSeconds;
                    var strToPrintOut = logCounter.PrintOutAndReset();
                    
                    if (strToPrintOut != null)
                        log.LogInformation(strToPrintOut);
                }
                else
                    countdownToPrintOut--;

                await Task.Delay(1000);
            }
        }, cancellationToken);

}
