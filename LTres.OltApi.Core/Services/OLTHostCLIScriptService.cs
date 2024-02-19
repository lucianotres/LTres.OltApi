using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LTres.OltApi.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LTres.OltApi.Core;

public class OLTHostCLIScriptService : IOLTHostCLIScriptService
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private IServiceScope? actualScope;
    private IOLTHostCLIActionsService? oltHostCLIActionsService;
    private Guid guidOltId = Guid.Empty;
    private Guid guidScriptId = Guid.Empty;
    private string scriptToRun = string.Empty;
    private IDictionary<string, string> scriptVariables;

    public bool ScriptCompleted { get; private set; } = false;
    public string ScriptResult { get; private set; } = string.Empty;

    public OLTHostCLIScriptService(IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        scriptVariables = new Dictionary<string, string>(0);
    }

    public async Task<bool> StartScript(Guid oltId, Guid scriptId, IDictionary<string, string>? variables)
    {
        await Task.Delay(1);

        guidOltId = oltId;
        guidScriptId = scriptId;

        scriptToRun = "$onuid\r\n$onuid";
        scriptVariables = variables ?? new Dictionary<string, string>(0);

        actualScope = serviceScopeFactory.CreateScope();
        if (actualScope == null)
        {
            ScriptResult = "Failed to create a scope to run script!";
            return false;
        }

        oltHostCLIActionsService = actualScope.ServiceProvider.GetRequiredService<IOLTHostCLIActionsService>();

        _ = RunScript().ConfigureAwait(false);
        return true;
    }

    private async Task RunScript()
    {
        if (actualScope == null || oltHostCLIActionsService == null)
            return;

        try
        {
            var scriptLines = scriptToRun.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
            for (int i = 0; i < scriptLines.Count; i++)
            {
                scriptLines[i] = await ProcessVariables(scriptLines[i]);
            }

            
        }
        finally
        {
            ScriptCompleted = true;
            actualScope.Dispose();
        }
    }

    private string? GetVariableValue(string variable)
    {
        if (scriptVariables.TryGetValue(variable, out string? variableValue))
            return variableValue;
        else
            return null;
    }

    private int? GetVariableValueInt(string variable)
    {
        var variableValue = GetVariableValue(variable);
        if (variableValue != null && int.TryParse(variableValue, out int variableIntValue))
            return variableIntValue;
        else
            return null;
    }

    private async Task<string> ProcessVariables(string line)
    {
        var variableExpression = new Regex(@"\$(\w+)");

        var matches = variableExpression.Matches(line);
        var matchedVariables = matches
            .Where(w => w.Success)
            .Select(s => s.Groups[1].Value.ToLower())
            .Distinct();

        foreach(var variable in matchedVariables)
        {
            string? variableValue = GetVariableValue(variable);

            if (variableValue == null)
            {
                variableValue = await TranslateVariableAsMethod(variable);

                if (variableValue != null)
                    scriptVariables.Add(variable, variableValue);
            }

            line = line.Replace(variable, variableValue, StringComparison.InvariantCultureIgnoreCase);
        }

        return line;
    }

    private async Task<string?> TranslateVariableAsMethod(string variable)
    {
        if (variable == "onuid")
        {
            var olt = GetVariableValueInt("olt");
            var slot = GetVariableValueInt("slot");
            var port = GetVariableValueInt("port");

            if (oltHostCLIActionsService != null && olt.HasValue && slot.HasValue && port.HasValue)
            {
                var unsuedIndex = await oltHostCLIActionsService.GetFirstUnusedOnuIndex(guidOltId, olt.Value, slot.Value, port.Value);
                return unsuedIndex.HasValue ? unsuedIndex.Value.ToString() : string.Empty;
            }
            else
                return null;
        }
        else
            return variable;
    }
}
