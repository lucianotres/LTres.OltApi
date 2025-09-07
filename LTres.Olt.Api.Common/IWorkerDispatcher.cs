using LTres.Olt.Api.Common.Models;

namespace LTres.Olt.Api.Common;

public interface IWorkerDispatcher
{
    /// <summary>
    /// Send a work to be done
    /// </summary>
    void Dispatch(WorkProbeInfo workInfo);
}
