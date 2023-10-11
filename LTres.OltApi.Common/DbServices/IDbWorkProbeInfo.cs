using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Common.DbServices;

public interface IDbWorkProbeInfo
{
    Task<IEnumerable<WorkProbeInfo>> ToDoList();
}