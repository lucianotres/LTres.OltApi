using System.Diagnostics;
using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;
using Microsoft.Extensions.Logging;
using SnmpSharpNet;

namespace LTres.OltApi.Snmp;

public class WorkSnmpWalkAction2 : IWorkerActionSnmpWalk
{
    private ILogger _log;

    public WorkSnmpWalkAction2(ILogger<WorkSnmpWalkAction> logger)
    {
        _log = logger;
    }

    public async Task<WorkProbeResponse> Execute(WorkProbeInfo probeInfo, CancellationToken cancellationToken, WorkProbeResponse? initialResponse = null)
    {
        var finalResponse = initialResponse ?? new WorkProbeResponse() { Id = probeInfo.Id };
        finalResponse.Success = false;

        try
        {
            var returnListOfVariables = new List<Vb>();
            var community = new OctetString(probeInfo.SnmpCommunity ?? "");
            var originalOID = new Oid(probeInfo.ItemKey ?? "");
            var latestOID = (Oid)originalOID.Clone();
            var bulkMessage = probeInfo.SnmpVersion > 1 && probeInfo.SnmpBulk;

            _log.LogDebug($"SNMP walk starting, {probeInfo.Host} -v {probeInfo.SnmpVersion} {(bulkMessage ? "bulk" : "")} -c {community} {latestOID}");
            var timer = Stopwatch.StartNew();

            var snmpAgentParams = new AgentParameters(probeInfo.SnmpVersion == 3 ? SnmpVersion.Ver3 : probeInfo.SnmpVersion == 2 ? SnmpVersion.Ver2 : SnmpVersion.Ver1,
                new OctetString(probeInfo.SnmpCommunity ?? ""));

            using var udpTarget = new UdpTarget(probeInfo.Host.Address, probeInfo.Host.Port,
                bulkMessage ? 20000 : 5000,
                bulkMessage ? 2 : 3);

            bool errorFound = false;
            do
            {
                var pdu = bulkMessage ? Pdu.GetBulkPdu() : Pdu.GetNextPdu();
                pdu.VbList.Add(latestOID);

                if (cancellationToken.IsCancellationRequested)
                    return finalResponse;

                var response = await Task.Run(() => udpTarget.Request(pdu, snmpAgentParams)).ConfigureAwait(false);

                errorFound = response.Pdu.ErrorStatus != 0;
                if (!errorFound)
                {
                    var toAdd = response.Pdu.VbList.Where(w =>
                        originalOID.IsRootOf(w.Oid) &&
                        !returnListOfVariables.Any(z => z.Oid == w.Oid))
                        .ToList();

                    if (toAdd.Count == 0)
                        break;

                    returnListOfVariables.AddRange(toAdd);

                    var found = response.Pdu.VbList.LastOrDefault();
                    if (found == null || !originalOID.IsRootOf(found.Oid))
                        break;

                    latestOID = (Oid)found.Oid.Clone();
                }
            }
            while (!errorFound && !cancellationToken.IsCancellationRequested);

            timer.Stop();

            if (cancellationToken.IsCancellationRequested)
                _log.LogDebug($"SNMP walk cancelled with {timer.Elapsed}");
            else
            {
                _log.LogDebug($"SNMP walk completed in {timer.Elapsed}");
                var reduceKeyAt = originalOID.ToString().Length + 1;

                finalResponse.Type = WorkProbeResponseType.Walk;
                finalResponse.ValueUInt = (uint)returnListOfVariables.Count;
                finalResponse.Values = returnListOfVariables.Select(variable =>
                {
                    var returnVar = new WorkProbeResponseVar()
                    {
                        Key = variable.Oid.ToString().Substring(reduceKeyAt)
                    };

                    if (variable.Value is Integer32 integer)
                        returnVar.ValueInt = integer.Value;
                    else if (variable.Value is Gauge32 gauge)
                        returnVar.ValueUInt = gauge.Value;
                    else if (variable.Value is Counter32 counter)
                        returnVar.ValueUInt = counter.Value;
                    else if (variable.Value is TimeTicks ticks)
                        returnVar.ValueUInt = ticks.Value;
                    else if (variable.Value is OctetString str)
                        returnVar.ValueStr = probeInfo.AsHex.GetValueOrDefault() ? str.ToHexString().Replace(" ", "") : str.ToString();
                    else if (variable.Value is Sequence binary)
                        returnVar.ValueStr = Convert.ToBase64String(binary.Value);

                    return returnVar;
                }).ToList();

                finalResponse.Success = true;
            }
        }
        catch (Exception error)
        { finalResponse.FailMessage = error.Message; }
        return finalResponse;
    }
}
