using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.Models;
using LTres.Olt.Api.Core.Tools;

namespace LTres.Olt.Api.Core.Workers;

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
        finalResponse.Type = WorkProbeResponseType.Walk;

        await Task.Delay(100);

        if (probeInfo.ItemKey != null)
        {
            var oidItems = _mockSNMPItems.StartWithOid(probeInfo.ItemKey);

            if (oidItems != null && oidItems.Any())
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
