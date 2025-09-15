using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.Models;

namespace LTres.Olt.Api.Core.Workers;

public class MockPingAction : IWorkerActionPing
{
    public async Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, CancellationToken cancellationToken, WorkProbeResponse? initialResponse = null)
    {
        var response = initialResponse ?? new WorkProbeResponse() { Id = probeInfo.Id };

        await Task.Delay(10);
        if (cancellationToken.IsCancellationRequested)
        {
            response.Success = false;
            return response;
        }   

        response.ValueInt = Random.Shared.Next(10, 90);
        response.Success = true;
        return response;
    }
}
