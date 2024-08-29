using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;
using LTres.OltApi.Core.Tools;

namespace LTres.OltApi.Core.Workers;

public class MockSnmpWalkAction : IWorkerActionSnmpWalk
{
    private MockSNMPItems _mockSNMPItems;

    public MockSnmpWalkAction(MockSNMPItems mockSNMPItems)
    {
        _mockSNMPItems = mockSNMPItems;
    }

    public async Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, CancellationToken cancellationToken, WorkProbeResponse? initialResponse = null)
    {
        var finalResponse = initialResponse ?? new WorkProbeResponse() { Id = probeInfo.Id };
        finalResponse.Success = false;

        await Task.Delay(300);

        if (probeInfo.ItemKey != null)
        {
            await Task.Delay(300);

            var oidItems = _mockSNMPItems.StartWithOid(probeInfo.ItemKey);

            if (oidItems != null)
            {
                finalResponse.ValueUInt = (uint)oidItems.Count();
                finalResponse.Values = oidItems.Select(s =>
                {
                    var workProbeResponseVar = new WorkProbeResponseVar()
                    {
                        Key = s.Key
                    };

                    if (s.Value.Type == 1)
                        workProbeResponseVar.ValueInt = s.Value.GetRandomValueInt();
                    else if (s.Value.Type == 2)
                        workProbeResponseVar.ValueUInt = s.Value.GetRandomValueUInt();
                    else
                        workProbeResponseVar.ValueStr = s.Value.GetRandomValueStr();

                    return workProbeResponseVar;
                }).ToList();

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
