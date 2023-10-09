using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Common;

public interface IWorkerDispatcher
{
    /// <summary>
    /// Send a work to be done
    /// </summary>
    void Dispatch(WorkProbeInfo workInfo);
}