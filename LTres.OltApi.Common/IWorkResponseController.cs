using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Common;

public interface IWorkResponseController
{
    Task ResponseReceived(WorkProbeResponse workProbeResponse);
}