using System.Collections.Concurrent;
using System.Text;
using LTres.OltApi.Common;
using Microsoft.VisualBasic;

namespace LTres.OltApi.Core;

public class LogCounter : ILogCounter
{
    private readonly ConcurrentBag<LogCounterData> logBag;
    private DateTime startedLoggingAt = DateTime.MinValue;

    public LogCounter()
    {
        logBag = new ConcurrentBag<LogCounterData>();
    }

    public void AddSuccess(Guid id, string category, TimeSpan? timeDone = null) =>
        InternalAdd(id, category, true, timeDone.GetValueOrDefault(TimeSpan.Zero), 1);

    public void AddError(Guid id, string category, TimeSpan? timeDone = null, Exception? error = null) =>
        InternalAdd(id, category, false, timeDone.GetValueOrDefault(TimeSpan.Zero), 1, error);

    public void AddCount(string category, int quantity, TimeSpan? timeDone = null) =>
        InternalAdd(Guid.NewGuid(), category, true, timeDone.GetValueOrDefault(TimeSpan.Zero), quantity);

    private void InternalAdd(Guid id, string category, bool sucess, TimeSpan timeDone, int quantity, Exception? error = null)
    {
        if (logBag.IsEmpty)
            startedLoggingAt = DateTime.Now;

        logBag.Add(new LogCounterData(
            Id: id,
            Category: category,
            Success: sucess,
            Quantity: quantity,
            TimeDone: timeDone,
            Error: error));
    }

    public string? PrintOutAndReset()
    {
        var loggingTimeDiff = DateTime.Now.Subtract(startedLoggingAt);
        var logTilNow = logBag.ToList();
        logBag.Clear();

        var groupByCategory = logTilNow.GroupBy(g => g.Category);
        var output = new StringBuilder();

        foreach (var group in groupByCategory)
        {
            var groupCount = group.Sum(s => s.Quantity);
            var itemPerSec = Math.Round(groupCount / loggingTimeDiff.TotalSeconds, 1);
            var toCalcAvgTime = group.Where(w => w.TimeDone > TimeSpan.Zero);
            var avgWorkTime = toCalcAvgTime.Any() ? Math.Round(toCalcAvgTime.Average(s => s.TimeDone.TotalMilliseconds) / 1000, 3) : 0;
            var successCount = group.Where(w => w.Success).Sum(s => s.Quantity);
            var failedCount = group.Where(w => !w.Success).Sum(s => s.Quantity);

            output.AppendLine($"{group.Key,20} {groupCount,-8} {successCount,-8} {failedCount,-8} {itemPerSec,-8} {avgWorkTime,-8}");
        }

        if (output.Length == 0)
            return null;
        else
        {
            output.Insert(0, $"{DateTime.Now,-20} {"total",-8} {"success",-8} {"fail",-8} {"per sec",-8} {"exec (s)",-8}\r\n");
            return output.ToString();
        }
    }

}

record LogCounterData(Guid Id, string Category, bool Success, int Quantity, TimeSpan TimeDone, Exception? Error);


public static class LogCounterExtensions
{
    public static Task RunPeriodicNotification(
        this LogCounter logCounter,
        CancellationToken cancellationToken,
        int everySeconds,
        Action<string> notificationAction) => Task.Run(async () =>
        {
            int countdownToPrintOut = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (countdownToPrintOut <= 0)
                {
                    countdownToPrintOut = everySeconds;
                    var strToPrintOut = logCounter.PrintOutAndReset();
                    
                    if (strToPrintOut != null)
                        notificationAction(strToPrintOut);
                }
                else
                    countdownToPrintOut--;

                await Task.Delay(1000);
            }
        }, cancellationToken);
}