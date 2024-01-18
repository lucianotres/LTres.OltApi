namespace LTres.OltApi.Common;

public interface IOLTHostCLIScriptService
{
    /// <summary>
    /// True once the script is completed
    /// </summary>
    bool ScriptCompleted { get; }

    /// <summary>
    /// Result of executed script
    /// </summary>
    string ScriptResult { get; }

    /// <summary>
    /// Start the execution of a script in background. False when fail to start.
    /// </summary>
    /// <param name="oltId">ID of OLT</param>
    /// <param name="scriptId">ID of the saved script to run</param>
    /// <param name="variables">Variables and values</param>
    Task<bool> StartScript(Guid oltId, Guid scriptId, IDictionary<string, string>? variables);
}