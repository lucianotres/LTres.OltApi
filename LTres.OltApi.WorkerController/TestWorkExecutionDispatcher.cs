using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;

namespace LTres.OltApi.WorkController;

public class TestWorkExecutionDispatcher : IWorkerDispatcher
{
    public void Dispatch(WorkProbeInfo workInfo)
    {
        Console.WriteLine($" --> {workInfo.Host} ping");
    }
}