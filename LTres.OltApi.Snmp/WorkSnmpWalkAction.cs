﻿using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Snmp;

public class WorkSnmpWalkAction : IWorkerActionSnmpWalk
{
    public async Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, WorkProbeResponse? initialResponse = null)
    {
        var finalResponse = initialResponse ?? new WorkProbeResponse() { Id = probeInfo.Id };
        finalResponse.Success = false;

        try
        {
            var returnListOfVariables = new List<Variable>();
            var community = new OctetString(probeInfo.SnmpCommunity ?? "");
            var originalOID = probeInfo.ItemKey ?? "";
            var latestOID = new ObjectIdentifier(originalOID);
            var bulkMessage = probeInfo.SnmpVersion > 1 && probeInfo.SnmpBulk;

            bool errorFound = false;
            do
            {
                var variables = new List<Variable> { new(latestOID) };

                if (bulkMessage)
                {
                    var message = new GetBulkRequestMessage(Messenger.NextRequestId,
                        probeInfo.SnmpVersion == 3 ? VersionCode.V3 : VersionCode.V2,
                        community,
                        0, 300, 
                        variables);
                    
                    var response = await message.GetResponseAsync(probeInfo.Host);
                    var pdu = response.Pdu();
                    errorFound = pdu.ErrorStatus.ToErrorCode() != 0;
                    if (!errorFound)
                    {
                        var toAdd = pdu.Variables.Where(w => 
                            w.Id.ToString().StartsWith(originalOID, StringComparison.Ordinal)
                            && !returnListOfVariables.Any(z => z.Id == w.Id))
                            .ToList();

                        returnListOfVariables.AddRange(toAdd);

                        var found = pdu.Variables.LastOrDefault();
                        if (found == null || !found.Id.ToString().StartsWith(originalOID, StringComparison.Ordinal))
                            break;

                        latestOID = found.Id;
                    }
                }
                else
                {
                    var message = new GetNextRequestMessage(
                        Messenger.NextRequestId,
                        probeInfo.SnmpVersion == 3 ? VersionCode.V3 : probeInfo.SnmpVersion == 2 ? VersionCode.V2 : VersionCode.V1,
                        community,
                        variables);

                    var response = await message.GetResponseAsync(probeInfo.Host);
                    var pdu = response.Pdu();
                    errorFound = pdu.ErrorStatus.ToErrorCode() != 0;
                    if (!errorFound)
                    {
                        var found = pdu.Variables[0];
                        if (!found.Id.ToString().StartsWith(originalOID, StringComparison.Ordinal)  //only table matches
                            || returnListOfVariables.Any(w => w.Id == found.Id))                    //prevent loop
                            break;

                        returnListOfVariables.Add(found);
                        latestOID = found.Id;
                    }
                }
            }
            while (!errorFound);

            finalResponse.Values = returnListOfVariables.Select(variableReply =>
            {
                var returnVar = new WorkProbeResponseVar()
                {
                    Key = variableReply.Id.ToString()
                };

                if (variableReply.Data is Integer32 integer)
                    returnVar.ValueInt = integer.ToInt32();
                else if (variableReply.Data is Gauge32 gauge)
                    returnVar.ValueUInt = gauge.ToUInt32();
                else if (variableReply.Data is Counter32 counter)
                    returnVar.ValueUInt = counter.ToUInt32();
                else if (variableReply.Data is TimeTicks ticks)
                    returnVar.ValueUInt = ticks.ToUInt32();
                else if (variableReply.Data is OctetString str)
                    returnVar.ValueStr = str.ToString();
                else if (variableReply.Data is Sequence binary)
                    returnVar.ValueBin = Convert.ToBase64String(binary.ToBytes());

                return returnVar;
            }).ToList();

            finalResponse.Success = true;
        }
        catch (Exception error)
        { finalResponse.FailMessage = error.Message; }
        return finalResponse;
    }
}
