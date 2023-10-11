using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Common;

public interface IWorkListController
{
    /// <summary>
    /// Return a list of work to be done
    /// </summary>
    Task<IEnumerable<WorkProbeInfo>> ToBeDone();
}