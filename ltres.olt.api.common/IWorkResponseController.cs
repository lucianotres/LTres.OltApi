using LTres.Olt.Api.Common.Models;

namespace LTres.Olt.Api.Common;

public interface IWorkResponseController
{
    Task ResponseReceived(WorkProbeResponse workProbeResponse);
}
