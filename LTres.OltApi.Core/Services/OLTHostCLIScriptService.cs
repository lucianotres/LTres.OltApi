using System.Linq;
using System.Text;
using LTres.OltApi.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LTres.OltApi.Core;

public class OLTHostCLIScriptService : IOLTHostCLIScriptService
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private IServiceScope? actualScope;
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

        scriptToRun = "";
        scriptVariables = variables ?? new Dictionary<string, string>(0);

        actualScope = serviceScopeFactory.CreateScope();
        if (actualScope == null)
        {
            ScriptResult = "Failed to create a scope to run script!";
            return false;
        }

        _ = RunScript().ConfigureAwait(false);
        return true;
    }

    private async Task RunScript()
    {
        if (actualScope == null)
            return;

        try
        {
            var oltHostActions = actualScope.ServiceProvider.GetRequiredService<IOLTHostCLIActionsService>();

            //some test operation here, need to implement the script interpretation
            var onuInfoResult = await oltHostActions.GetONUInfo(guidOltId, 1, 1, 2, 5);
            if (onuInfoResult != null)
                ScriptResult = onuInfoResult.Aggregate(new StringBuilder(), (sb, s) =>
                {
                    sb.AppendLine(s);
                    return sb;
                }).ToString();
        }
        finally
        {
            ScriptCompleted = true;
            actualScope.Dispose();
        }
    }

}
