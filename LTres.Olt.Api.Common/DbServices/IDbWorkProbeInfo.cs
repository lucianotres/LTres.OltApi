using LTres.Olt.Api.Common.Models;

namespace LTres.Olt.Api.Common.DbServices;

public interface IDbWorkProbeInfo
{
    Task<IEnumerable<WorkProbeInfo>> ToDoList();
}
