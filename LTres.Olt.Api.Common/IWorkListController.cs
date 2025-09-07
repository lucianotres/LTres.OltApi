using LTres.Olt.Api.Common.Models;

namespace LTres.Olt.Api.Common;

public interface IWorkListController
{
    /// <summary>
    /// Return a list of work to be done
    /// </summary>
    Task<IEnumerable<WorkProbeInfo>> ToBeDone();
}
