using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Common;

public interface IOLTHostItemService
{
    Task<Guid> AddOLTHostItem(OLT_Host_Item item);

    Task<int> ChangeOLTHostItem(OLT_Host_Item item);

    Task<IEnumerable<OLT_Host_Item>> ListOLTHostItems(int take = 1000, int skip = 0,
        Guid? filterByOlt = null,
        Guid? filterById = null,
        bool? filterActive = null,
        bool? filterTemplate = null,
        Guid? filterRelated = null,
        string? filterKey = null);

    Task<IEnumerable<ONU_Info>> ListONUInfo(Guid olt, bool full = true);
}
