using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;
using org.matheval;
using org.matheval.Functions;

namespace LTres.OltApi.Core;

public class WorkProbeCalc2Values : IWorkProbeCalc
{
    public async Task UpdateProbedValuesWithCalculated(WorkProbeInfo workProbeInfo, WorkProbeResponse workProbeResponse) =>
        await Task.Run(() => Calculate(workProbeInfo, workProbeResponse));

    private void Calculate(WorkProbeInfo workProbeInfo, WorkProbeResponse workProbeResponse)
    {
        if (workProbeInfo.Calc == null)
            return;

        var expression = new Expression(workProbeInfo.Calc);
        //sample:
        //IF(val <= 15000 && val >= -15000, (val * 0.2) - 3000, 99999)
        
        if (workProbeResponse.Type == WorkProbeResponseType.Value)
        {
            if (workProbeResponse.ValueInt.HasValue)
            {
                expression.Bind("val", workProbeResponse.ValueInt.Value);
                workProbeResponse.ValueInt = expression.Eval<int>();

                if (workProbeResponse.ValueInt.Value == int.MaxValue)
                    workProbeResponse.ValueInt = null;
            }
            else if (workProbeResponse.ValueUInt.HasValue)
            {
                expression.Bind("val", workProbeResponse.ValueUInt.Value);
                workProbeResponse.ValueUInt = expression.Eval<uint>();

                if (workProbeResponse.ValueUInt.Value == uint.MaxValue)
                    workProbeResponse.ValueUInt = null;
            }
        }
        else if (workProbeResponse.Type == WorkProbeResponseType.Walk)
        {
            if (workProbeResponse.Values != null)
                foreach (var v in workProbeResponse.Values)
                {
                    if (v.ValueInt.HasValue)
                    {
                        expression.Bind("val", v.ValueInt.Value);
                        v.ValueInt = expression.Eval<int>();

                        if (v.ValueInt.Value == int.MaxValue)
                            v.ValueInt = null;
                    }
                    else if (v.ValueUInt.HasValue)
                    {
                        expression.Bind("val", v.ValueUInt.Value);
                        v.ValueUInt = expression.Eval<uint>();

                        if (v.ValueUInt.Value == uint.MaxValue)
                            v.ValueUInt = null;
                    }
                }
        }
    }
}
