using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Common.DbServices;

public interface IDbWorkProbeResponse
{
    Task SaveWorkProbeResponse(WorkProbeResponse workProbeResponse);

    Task<IEnumerable<OLT_Host_Item>> GetItemTemplates(Guid from);

    Task CreateItemsFromTemplate(OLT_Host_Item itemTemplate, WorkProbeResponse workProbeResponse);
}