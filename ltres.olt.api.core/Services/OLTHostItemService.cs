using System.Data.Common;
using System.Numerics;
using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.Models;
using Microsoft.Extensions.Logging;

namespace LTres.Olt.Api.Core;

public class OLTHostItemService : IOLTHostItemService
{
    private readonly IDbOLTHostItem _db;
    private readonly ILogger _log;

    public OLTHostItemService(ILogger<OLTHostItemService> logger, IDbOLTHostItem dbOLTHostItem)
    {
        _log = logger;
        _db = dbOLTHostItem;
    }

    private void ValidateOltHostItem(OLT_Host_Item item)
    {
        if (item == null)
            throw new ArgumentNullException("item");
        if (!item.IdOltHost.HasValue || item.IdOltHost.Value == Guid.Empty)
            throw new ArgumentNullException("IdOltHost", "OLT item should have a related OLT!");

        if (string.IsNullOrWhiteSpace(item.Action))
            throw new ArgumentNullException("Action", "OLT item should have an action!");
        else if (!OLT_Host_ItemExtensions.ValidActions.Contains(item.Action))
            throw new ArgumentOutOfRangeException("Action", $"OLT item should have a valid action {OLT_Host_ItemExtensions.ValidActions}!");

        var interval = item.Interval.GetValueOrDefault();
        if (interval < OLT_Host_ItemExtensions.MinInterval || interval > OLT_Host_ItemExtensions.MaxInterval)
            throw new ArgumentOutOfRangeException("Interval", $"OLT item should have an interval between {OLT_Host_ItemExtensions.MinInterval} and {OLT_Host_ItemExtensions.MaxInterval} seconds.");

        interval = item.HistoryFor.GetValueOrDefault();
        if (item.HistoryFor.HasValue && (interval < OLT_Host_ItemExtensions.MinHistoryFor || interval > OLT_Host_ItemExtensions.MaxHistoryFor))
            throw new ArgumentOutOfRangeException("HistoryFor", $"OLT item can have history recorded by {OLT_Host_ItemExtensions.MinHistoryFor} up to {OLT_Host_ItemExtensions.MaxHistoryFor} minutes.");

        if (item.MaintainFor.HasValue && !item.Template.GetValueOrDefault())
            throw new ArgumentOutOfRangeException("MaintainFor", "MaintainFor can only be used when OLT Item it's a template!");

        interval = item.MaintainFor.GetValueOrDefault();
        if (item.MaintainFor.HasValue && (interval < OLT_Host_ItemExtensions.MinMaintainFor || interval > OLT_Host_ItemExtensions.MaxMaintainFor))
            throw new ArgumentOutOfRangeException("MaintainFor", $"Discovered item can be maintained by {OLT_Host_ItemExtensions.MinMaintainFor} up to {OLT_Host_ItemExtensions.MaxMaintainFor} minutes.");


        if (item.IdRelated.HasValue)
            throw new ArgumentOutOfRangeException("IdRelated", "IdRelated can't be informed by user, that will be filled automaticlly by the system when needed!");

        if (item.Template.GetValueOrDefault() && (!item.From.HasValue || item.From == Guid.Empty))
            throw new ArgumentNullException("From", "When an OLT Item is marked as template, that should have 'From' informed!");
    }

    public async Task<Guid> AddOLTHostItem(OLT_Host_Item item)
    {
        ValidateOltHostItem(item);

        if (item.Id != Guid.Empty)
            throw new ArgumentOutOfRangeException("Id", "Id should be empty when adding!");

        var registeredGuid = await _db.AddOLTHostItem(item);
        _log.LogInformation($"New OLT_Host_Item included: {registeredGuid}");
        return registeredGuid;

    }

    public async Task<int> ChangeOLTHostItem(OLT_Host_Item item)
    {
        ValidateOltHostItem(item);

        if (item.Id == Guid.Empty)
            throw new ArgumentOutOfRangeException("Id", "Id should not be empty when changing!");

        return await _db.ChangeOLTHostItem(item);
    }

    public async Task<IEnumerable<OLT_Host_Item>> ListOLTHostItems(int take = 1000, int skip = 0,
        Guid? filterByOlt = null,
        Guid? filterById = null,
        bool? filterActive = null,
        bool? filterTemplate = null,
        Guid? filterRelated = null,
        string? filterKey = null)
    {
        if (take < 0 || take > 999999)
            throw new ArgumentOutOfRangeException("take");
        if (skip < 0)
            throw new ArgumentOutOfRangeException("skip");

        return await _db.ListOLTHostItems(take, skip, filterByOlt, filterById, filterActive, filterTemplate, filterRelated, filterKey);
    }

    public async Task<IEnumerable<ONU_Info>> ListONUInfo(Guid olt, bool full = true)
    {
        List<ONU_Info> finalList = new();
        Dictionary<string, string?> lstOnuSn = new();
        Dictionary<string, string?> lstOnuDesc = new();
        Dictionary<string, int?> lstOnuDistance = new();
        Dictionary<string, int?> lstOnuSt = new();
        Dictionary<string, int?> lstOnuRx;

        var onuRefs = await _db.GetOLTOnuRef(olt);
        if (onuRefs == null || onuRefs.Name == null || onuRefs.Signal == null)
            return finalList;

        finalList = (await _db.ListOLTHostItems(999999, 0, null, null, null, null, onuRefs.Name))
            .Where(w => w.ItemKey != null)
            .Select(s => new ONU_Info()
            {
                key = s.ItemKey ?? "",
                name = full ? s.ProbedValueStr : null
            })
            .ToList();

        if (full && onuRefs.SN != null)
            lstOnuSn = (await _db.ListOLTHostItems(999999, 0, null, null, null, null, onuRefs.SN))
                .Where(w => w.ItemKey != null)
                .ToDictionary(k => k.ItemKey ?? "", s => s.ProbedValueStr);

        if (full && onuRefs.Desc != null)
            lstOnuDesc = (await _db.ListOLTHostItems(999999, 0, null, null, null, null, onuRefs.Desc))
                .Where(w => w.ItemKey != null)
                .ToDictionary(k => k.ItemKey ?? "", s => s.ProbedValueStr);

        if (full && onuRefs.Distance != null)
            lstOnuDistance = (await _db.ListOLTHostItems(999999, 0, null, null, null, null, onuRefs.Distance))
                .Where(w => w.ItemKey != null)
                .ToDictionary(k => k.ItemKey ?? "", s => s.ProbedValueInt);

        if (onuRefs.State != null)
            lstOnuSt = (await _db.ListOLTHostItems(999999, 0, null, null, null, null, onuRefs.State))
                .Where(w => w.ItemKey != null)
                .ToDictionary(k => k.ItemKey ?? "", s => s.ProbedValueInt);

        lstOnuRx = (await _db.ListOLTHostItems(999999, 0, null, null, null, null, onuRefs.Signal))
            .Where(w => w.ItemKey != null)
            .ToDictionary(k => k.ItemKey ?? "", s => s.ProbedValueInt);

        foreach (var n in finalList)
        {
            if (full && lstOnuSn.TryGetValue(n.key, out string? sn))
                n.sn = sn;

            if (full && lstOnuDesc.TryGetValue(n.key, out string? desc))
                n.desc = desc;

            if (full && lstOnuDistance.TryGetValue(n.key, out int? distance))
                n.distance = distance;

            if (lstOnuSt.TryGetValue(n.key, out int? st))
                n.state = st;

            if (lstOnuRx.TryGetValue(n.key, out int? rx))
                n.rx = rx;
        };

        return finalList;
    }

}
