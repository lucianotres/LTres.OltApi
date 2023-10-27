using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Snmp;

public class WorkSnmpGetAction : IWorkerActionSnmpGet
{
    public async Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, CancellationToken cancellationToken, WorkProbeResponse? initialResponse = null)
    {
        var finalResponse = initialResponse ?? new WorkProbeResponse() { Id = probeInfo.Id };
        finalResponse.Success = false;

        try
        {
            var requestMessage = new GetRequestMessage(
                Messenger.NextRequestId,
                probeInfo.SnmpVersion == 3 ? VersionCode.V3 : probeInfo.SnmpVersion == 2 ? VersionCode.V2 : VersionCode.V1,
                new OctetString(probeInfo.SnmpCommunity ?? ""),
                new List<Variable> { new(new ObjectIdentifier(probeInfo.ItemKey ?? "")) });

            if (cancellationToken.IsCancellationRequested)
                return finalResponse;

            var replyMessage = await Task.Run(() =>
                requestMessage.GetResponse(5000, probeInfo.Host),
                cancellationToken)
                .ConfigureAwait(false);

            var pdu = replyMessage.Pdu();
            var errorStatus = pdu.ErrorStatus.ToErrorCode();

            if (cancellationToken.IsCancellationRequested)
                return finalResponse;

            if (errorStatus == ErrorCode.NoError)
            {
                var variableReply = pdu.Variables.First();
                if (variableReply.Data is Integer32 integer)
                    finalResponse.ValueInt = integer.ToInt32();
                else if (variableReply.Data is Gauge32 gauge)
                    finalResponse.ValueUInt = gauge.ToUInt32();
                else if (variableReply.Data is Counter32 counter)
                    finalResponse.ValueUInt = counter.ToUInt32();
                else if (variableReply.Data is TimeTicks ticks)
                    finalResponse.ValueUInt = ticks.ToUInt32();
                else if (variableReply.Data is OctetString str)
                    finalResponse.ValueStr = str.ToString();
                else if (variableReply.Data is Sequence binary)
                    finalResponse.ValueStr = Convert.ToBase64String(binary.ToBytes());

                finalResponse.Success = true;
            }
            else if (errorStatus == ErrorCode.NoSuchName)
                finalResponse.FailMessage = $"Snmp object not founded, {errorStatus}";
            else
                finalResponse.FailMessage = $"Snmp get failed, {errorStatus}";
        }
        catch (Exception error)
        { finalResponse.FailMessage = error.Message; }

        return finalResponse;
    }
}
