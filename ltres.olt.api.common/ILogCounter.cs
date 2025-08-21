namespace LTres.Olt.Api.Common;

public interface ILogCounter
{
    void AddCount(string category, int quantity, TimeSpan? timeDone = null);

    void AddSuccess(Guid id, string category, TimeSpan? timeDone = null);

    void AddError(Guid id, string category, TimeSpan? timeDone = null, Exception? error = null);

    string? PrintOutAndReset();

    void RegisterHookOnPrintResetAction<T>(Action<ILogCounter> hookAction) where T : class;
}
