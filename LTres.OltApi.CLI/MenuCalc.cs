
using System.Net;
using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;
using LTres.OltApi.Core;

namespace LTres.OltApi.CLI;

public class MenuCalc : Menu
{
    public MenuCalc()
    {
        Description = "-- Testing the calc expressions evaluator --";
        Options.Add(new MenuOption('1', "Test an expression", TestingAnExpression));
        Options.Add(new MenuOption('r', "to return"));
    }

    private async Task<bool> TestingAnExpression()
    {
        Console.Write("What version should be used? (1,2) ");
        var version = Console.ReadLine()?.Trim();
        
        Console.Write("Inform the original value (int): ");
        var strOriginalValue = Console.ReadLine();
        if (!int.TryParse(strOriginalValue, out int originalValue))
            originalValue = 0;

        Console.Write("Inform the expression (where 'val' is a variable): ");
        var expression = Console.ReadLine();

        IWorkProbeCalc workCalc = version == "2" ? new WorkProbeCalc2Values() : new WorkProbeCalcValues();
        var mockProbeInfo = new WorkProbeInfo()
        {
            Id = Guid.NewGuid(),
            Host = new IPEndPoint(IPAddress.Loopback, 1),
            LastProbed = DateTime.Now,
            Calc = expression
        };
        var mockProbeResponse = new WorkProbeResponse()
        {
            Id = mockProbeInfo.Id,
            Success = true,
            Type = WorkProbeResponseType.Value,
            ProbedAt = DateTime.Now,
            ValueInt = originalValue
        };

        try
        {
            await workCalc.UpdateProbedValuesWithCalculated(mockProbeInfo, mockProbeResponse);

            Console.WriteLine($"Result: {mockProbeResponse.ValueInt}");
        }
        catch (Exception error)
        {
            Console.WriteLine(error);
        }

        return false;
    }
}
