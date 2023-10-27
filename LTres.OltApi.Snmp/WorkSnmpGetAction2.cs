using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;
using SnmpSharpNet;

namespace LTres.OltApi.Snmp;

public class WorkSnmpGetAction2 : IWorkerActionSnmpGet
{
    public async Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, CancellationToken cancellationToken, WorkProbeResponse? initialResponse = null)
    {
        var finalResponse = initialResponse ?? new WorkProbeResponse() { Id = probeInfo.Id };
        finalResponse.Success = false;

        try
        {
            var snmpAgentParams = new AgentParameters(probeInfo.SnmpVersion == 3 ? SnmpVersion.Ver3 : probeInfo.SnmpVersion == 2 ? SnmpVersion.Ver2 : SnmpVersion.Ver1,
                new OctetString(probeInfo.SnmpCommunity ?? ""));

            using var udpTarget = new UdpTarget(probeInfo.Host.Address, probeInfo.Host.Port, 5000, 3);

            var oid = new Oid(probeInfo.ItemKey ?? "");

            var pdu = Pdu.GetPdu();
            pdu.VbList.Add(oid);

            if (cancellationToken.IsCancellationRequested)
                return finalResponse;

            var response = await Task.Run(() => udpTarget.Request(pdu, snmpAgentParams)).ConfigureAwait(false);
            var errorStatus = response.Pdu.ErrorStatus;

            if (cancellationToken.IsCancellationRequested)
                return finalResponse;

            if (errorStatus == 0)
            {
                var variable = response.Pdu.VbList.First();

                if (variable.Value is Integer32 integer)
                    finalResponse.ValueInt = integer.Value;
                else if (variable.Value is Gauge32 gauge)
                    finalResponse.ValueUInt = gauge.Value;
                else if (variable.Value is Counter32 counter)
                    finalResponse.ValueUInt = counter.Value;
                else if (variable.Value is TimeTicks ticks)
                    finalResponse.ValueUInt = ticks.Value;
                else if (variable.Value is OctetString str)
                    finalResponse.ValueStr = str.ToString();
                else if (variable.Value is Sequence binary)
                    finalResponse.ValueStr = Convert.ToBase64String(binary.Value);

                finalResponse.Success = true;
            }
            else if (errorStatus == 2)
                finalResponse.FailMessage = $"Snmp object not founded, {errorStatus}";
            else
                finalResponse.FailMessage = $"Snmp get failed, {errorStatus}";
        }
        catch (Exception error)
        { finalResponse.FailMessage = error.Message; }

        return finalResponse;
    }
}
