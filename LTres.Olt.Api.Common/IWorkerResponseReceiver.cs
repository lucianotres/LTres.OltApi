using LTres.Olt.Api.Common.Models;

namespace LTres.Olt.Api.Common;

public interface IWorkerResponseReceiver
{
    event EventHandler<WorkerResponseReceivedEventArgs> OnResponseReceived;
}

public class WorkerResponseReceivedEventArgs : EventArgs
{
    public required WorkProbeResponse ProbeResponse { get; set; }
}
