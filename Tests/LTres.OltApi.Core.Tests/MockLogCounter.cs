using LTres.OltApi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTres.OltApi.Core.Tests;

public class MockLogCounter : ILogCounter
{
    public void AddCount(string category, int quantity, TimeSpan? timeDone = null) { }

    public void AddError(Guid id, string category, TimeSpan? timeDone = null, Exception? error = null) { }

    public void AddSuccess(Guid id, string category, TimeSpan? timeDone = null) { }

    public string? PrintOutAndReset() => null;

    public void RegisterHookOnPrintResetAction<T>(Action<ILogCounter> hookAction) where T : class { }
    
}

