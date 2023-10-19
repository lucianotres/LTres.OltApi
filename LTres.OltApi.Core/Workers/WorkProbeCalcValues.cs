using System.Runtime.CompilerServices;
using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;
using SimpleExpressionEvaluator;

namespace LTres.OltApi.Core;

public class WorkProbeCalcValues : IWorkProbeCalc
{
    public async Task UpdateProbedValuesWithCalculated(WorkProbeInfo workProbeInfo, WorkProbeResponse workProbeResponse) =>
        await Task.Run(() => Calculate(workProbeInfo, workProbeResponse));

    private void Calculate(WorkProbeInfo workProbeInfo, WorkProbeResponse workProbeResponse)
    {
        if (workProbeInfo.Calc == null)
            return;

        var expressionEvaluator = new ExpressionEvaluator();
        var mathParser = expressionEvaluator.Compile(workProbeInfo.Calc);
        
        if (workProbeResponse.Type == WorkProbeResponseType.Value)
        {
            if (workProbeResponse.ValueInt.HasValue)
            {
                var calculatedResult = mathParser(new { val = workProbeResponse.ValueInt.Value });
                workProbeResponse.ValueInt = Convert.ToInt32(calculatedResult);
            }
            else if (workProbeResponse.ValueUInt.HasValue)
            {
                var calculatedResult = mathParser(new { val = workProbeResponse.ValueUInt.Value });
                workProbeResponse.ValueUInt = Convert.ToUInt32(calculatedResult);
            }
        }
        else if (workProbeResponse.Type == WorkProbeResponseType.Walk)
        {
            if (workProbeResponse.Values != null)
                foreach (var v in workProbeResponse.Values)
                {
                    if (v.ValueInt.HasValue)
                    {
                        var calculatedResult = mathParser(new { val = v.ValueInt.Value });
                        v.ValueInt = Convert.ToInt32(calculatedResult);
                    }
                    else if (v.ValueUInt.HasValue)
                    {
                        var calculatedResult = mathParser(new { val = v.ValueUInt.Value });
                        v.ValueUInt = Convert.ToUInt32(calculatedResult);
                    }
                }
        }
    }
}
