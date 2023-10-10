using System.Collections.Generic;
using System.Net;
using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;

namespace LTres.OltApi.WorkController;

public class TestWorkList : IWorkListController, IWorkResponseController
{
    public IEnumerable<WorkProbeInfo> ToBeDone()
    {
        return new WorkProbeInfo[] 
        {
            new WorkProbeInfo() 
            {
                Id = Guid.NewGuid(),
                Action = "ping",
                Host = new IPEndPoint(IPAddress.Parse("201.150.15.134"), 1234),
                LastProbed = DateTime.Now,
                WaitingResponse = false 
            }
        };
    }

    public async Task ResponseReceived(WorkProbeResponse workProbeResponse)
    {
        Console.WriteLine($"RESPONSE: {workProbeResponse} -> {workProbeResponse.ValueInt}ms");
        await Task.Delay(1);
    }
}
