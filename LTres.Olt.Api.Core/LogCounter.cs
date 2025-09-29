using System.Collections.Concurrent;
using System.Text;
using LTres.Olt.Api.Common;

namespace LTres.Olt.Api.Core;

public class LogCounter : ILogCounter
{
    private readonly ConcurrentBag<LogCounterData> logBag = [];
    private readonly Dictionary<Type, Action<ILogCounter>> hooksPrintResetAction = [];
    private DateTime startedLoggingAt = DateTime.MinValue;

    public void AddSuccess(Guid id, string category, TimeSpan? timeDone = null) =>
        InternalAdd(id, category, true, timeDone.GetValueOrDefault(TimeSpan.Zero), 1);

    public void AddError(Guid id, string category, TimeSpan? timeDone = null, Exception? error = null) =>
        InternalAdd(id, category, false, timeDone.GetValueOrDefault(TimeSpan.Zero), 1, error);

    public void AddCount(string category, int quantity, TimeSpan? timeDone = null) =>
        InternalAdd(Guid.NewGuid(), category, null, timeDone.GetValueOrDefault(TimeSpan.Zero), quantity);

    private void InternalAdd(Guid id, string category, bool? success, TimeSpan timeDone, int quantity, Exception? error = null)
    {
        if (logBag.IsEmpty)
            startedLoggingAt = DateTime.Now;

        logBag.Add(new LogCounterData(
            Id: id,
            Category: category,
            Success: success,
            Quantity: quantity,
            TimeDone: timeDone,
            Error: error));
    }

    internal IEnumerable<LogCounterData> GetLogCounters() => logBag.ToList();

    public void RegisterHookOnPrintResetAction<T>(Action<ILogCounter> hookAction) where T : class
    {
        var type = typeof(T);
        if (hooksPrintResetAction.ContainsKey(type))
            hooksPrintResetAction[type] = hookAction;
        else
            hooksPrintResetAction.Add(type, hookAction);
    }

    private void RunHooksPrintResetActions()
    {
        foreach (var hookAction in hooksPrintResetAction.Values)
        {
            try
            { hookAction(this); }
            catch { }
        }
    }

    public string? PrintOutAndReset()
    {
        RunHooksPrintResetActions();

        var loggingTimeDiff = DateTime.Now.Subtract(startedLoggingAt);
        var logTilNow = logBag.ToList();
        logBag.Clear();

        var groupByCategory = logTilNow.GroupBy(g => g.Category);
        var output = new StringBuilder();

        foreach (var group in groupByCategory)
        {
            var groupCount = group.Sum(s => s.Quantity);
            var itemPerSec = group.Any(w => w.Success.HasValue) ? Math.Round(groupCount / loggingTimeDiff.TotalSeconds, 1) : 0;
            var toCalcAvgTime = group.Where(w => w.TimeDone > TimeSpan.Zero);
            var avgWorkTime = toCalcAvgTime.Any() ? Math.Round(toCalcAvgTime.Average(s => s.TimeDone.TotalMilliseconds) / 1000, 3) : 0;
            var successCount = group.Where(w => w.Success.HasValue && w.Success.Value).Sum(s => s.Quantity);
            var failedCount = group.Where(w => w.Success.HasValue && !w.Success.Value).Sum(s => s.Quantity);

            output.AppendLine($"{group.Key,15} {groupCount,-8} {successCount,-8} {failedCount,-8} {itemPerSec,-8} {avgWorkTime,-8}");
        }

        if (output.Length == 0)
            return null;
        
        output.Insert(0, $"{"",15} {"total",-8} {"success",-8} {"fail",-8} {"per sec",-8} {"exec (s)",-8}\r\n");
        return output.ToString();
    }

}

internal record LogCounterData(Guid Id, string Category, bool? Success, int Quantity, TimeSpan TimeDone, Exception? Error);
