using System.Net.NetworkInformation;
using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Core.Workers;

public class WorkPingAction : IWorkerActionPing
{
    public async Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, CancellationToken cancellationToken, WorkProbeResponse? initialResponse = null)
    {
        var response = initialResponse ?? new WorkProbeResponse() { Id = probeInfo.Id };

        try
        {
            using var ping = new Ping();
            var pingOptions = new PingOptions(10, true);
            var buffer = new byte[] 
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
                0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f 
            };

            var reply = await ping.SendPingAsync(probeInfo.Host.Address, TimeSpan.FromSeconds(5), buffer, pingOptions, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
            {
                response.Success = reply.Status == IPStatus.Success;
                response.ValueInt = (int)reply.RoundtripTime;
            }
        }
        catch
        {
            response.Success = false;
        }

        return response;
    }
}