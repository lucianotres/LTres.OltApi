using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Snmp;

public class WorkSnmpGetAction : IWorkerActionSnmpGet
{
    public async Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, WorkProbeResponse? initialResponse = null)
    {
        var finalResponse = initialResponse ?? new WorkProbeResponse() { Id = probeInfo.Id };
        finalResponse.Success = false;

        try
        {
            var requestMessage = new GetRequestMessage(
                Messenger.NextRequestId, VersionCode.V1,
                new OctetString(probeInfo.SnmpCommunity ?? ""),
                new List<Variable> { new Variable(new ObjectIdentifier(probeInfo.ItemKey ?? "")) });

            var replyMessage = await requestMessage.GetResponseAsync(probeInfo.Host);
            var pdu = replyMessage.Pdu();

            if (pdu.ErrorStatus.ToInt32() == 0)
            {
                var variableReply = pdu.Variables.First();
                if (variableReply.Data is Integer32)
                    finalResponse.ValueInt = ((Integer32)variableReply.Data).ToInt32();
                else if (variableReply.Data is Gauge32)
                    finalResponse.ValueUInt = ((Gauge32)variableReply.Data).ToUInt32();
                else if (variableReply.Data is Counter32)
                    finalResponse.ValueUInt = ((Counter32)variableReply.Data).ToUInt32();
                //else if (variableReply.Data is Sequence)
                //    finalResponse.ValueUInt = ((Sequence) variableReply.Data).ToBytes();
            }

            finalResponse.Success = true;
        }
        catch (Exception error)
        { finalResponse.FailMessage = error.Message; }

        return finalResponse;
    }
}
