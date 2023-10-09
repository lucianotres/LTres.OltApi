using System.Collections.Generic;
using System.Net;
using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;

namespace LTres.OltApi.WorkController;

public class TestWorkList : IWorkListController
{
    public IEnumerable<WorkProbeInfo> ToBeDone()
    {
        return new WorkProbeInfo[] 
        {
            new WorkProbeInfo() 
            {
                Id = new Guid(),
                Host = new IPEndPoint(IPAddress.Loopback, 1234),
                LastProbed = DateTime.Now,
                WaitingResponse = false 
            }
        };
    }
}
