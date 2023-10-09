using System.Net.NetworkInformation;
using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Core.Workers;

public class WorkPingAction : IWorkerActionPing
{
    public async Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, WorkProbeResponse? initialResponse = null)
    {
        var response = initialResponse ?? new WorkProbeResponse() { Id = probeInfo.Id };

        try
        {
            using var ping = new Ping();
            var pingOptions = new PingOptions(10, true);
            var buffer = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };

            var reply = await ping.SendPingAsync(probeInfo.Host.Address, 5000, buffer, pingOptions);

            response.Success = reply.Status == IPStatus.Success;
            response.ValueInt = (int)reply.RoundtripTime;
        }
        catch
        {
            response.Success = false;
        }

        return response;
    }
}