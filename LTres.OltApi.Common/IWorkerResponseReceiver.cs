using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Common;

public interface IWorkerResponseReceiver
{
    event EventHandler<WorkerResponseReceivedEventArgs> OnResponseReceived;
}

public class WorkerResponseReceivedEventArgs : EventArgs
{
    public required WorkProbeResponse ProbeResponse { get; set; }
}