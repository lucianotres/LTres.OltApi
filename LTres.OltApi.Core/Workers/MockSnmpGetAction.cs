using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;
using LTres.OltApi.Core.Tools;

namespace LTres.OltApi.Core.Workers;

public class MockSnmpGetAction : IWorkerActionSnmpGet
{
    private MockSNMPItems _mockSNMPItems;

    public MockSnmpGetAction(MockSNMPItems mockSNMPItems)
    {
        _mockSNMPItems = mockSNMPItems;
    }

    public async Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, CancellationToken cancellationToken, WorkProbeResponse? initialResponse = null)
    {
        var finalResponse = initialResponse ?? new WorkProbeResponse() { Id = probeInfo.Id };
        finalResponse.Success = false;

        if (probeInfo.ItemKey != null)
        {
            await Task.Delay(300);

            var oidItem = _mockSNMPItems[probeInfo.ItemKey];

            if (oidItem != null)
            {
                if (oidItem.Type == 1)
                    finalResponse.ValueInt = oidItem.GetRandomValueInt();
                else if (oidItem.Type == 2)
                    finalResponse.ValueUInt = oidItem.GetRandomValueUInt();
                else
                    finalResponse.ValueStr = oidItem.GetRandomValueStr();

                finalResponse.Success = true;
            }
            else
                finalResponse.FailMessage = "Oid not found!";
        }
        else
            finalResponse.FailMessage = "Invalid request, ItemKey required!";

        return finalResponse;
    }
}
