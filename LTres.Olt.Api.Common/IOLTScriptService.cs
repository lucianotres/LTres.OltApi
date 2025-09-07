using LTres.Olt.Api.Common.Models;

namespace LTres.Olt.Api.Common;

public interface IOLTScriptService
{
    /// <summary>
    /// Include an OLT script at DB.
    /// Returns: ID of the created registry
    /// </summary>
    Task<Guid> AddOLTScript(OLT_Script oltScript);

    /// <summary>
    /// Get a list of OLL scripts. Can use one or more filters to locate one of them
    /// </summary>
    /// <param name="take">Limit the result to max of records</param>
    /// <param name="skip">Skip a number of records before</param>
    /// <param name="filterId">Filter by ID</param>
    /// <param name="filterTag">Filter by existing tags</param>
    /// <returns></returns>
    Task<IEnumerable<OLT_Script>> ListOLTScripts(int take = 1000, int skip = 0,
        Guid? filterId = null,
        string[]? filterTag = null);


    /// <summary>
    /// Change an entire register of a OLT script.
    /// Id it's the key to find the existing one.
    /// </summary>
    /// <param name="oltScript">New OLT script info with the original ID</param>
    /// <returns>0 to nothing changed and positive for number of affected</returns>
    Task<int> ChangeOLTScript(OLT_Script oltScript);
}
