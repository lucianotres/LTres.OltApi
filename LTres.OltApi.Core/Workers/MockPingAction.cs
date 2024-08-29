using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Core.Workers;

public class MockPingAction : IWorkerActionPing
{
    public async Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, CancellationToken cancellationToken, WorkProbeResponse? initialResponse = null)
    {
        var response = initialResponse ?? new WorkProbeResponse() { Id = probeInfo.Id };

        await Task.Delay(10);

        response.ValueInt = Random.Shared.Next(10, 90);
        response.Success = true;
        return response;
    }
}
