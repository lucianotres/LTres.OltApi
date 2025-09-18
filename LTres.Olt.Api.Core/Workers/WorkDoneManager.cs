using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.DbServices;
using LTres.Olt.Api.Common.Models;

namespace LTres.Olt.Api.Core.Workers;

public class WorkDoneManager(IWorkProbeCache workProbeCache, IDbWorkProbeResponse dbWorkProbeResponse) : IWorkResponseController
{
    private readonly IWorkProbeCache _workProbeCache = workProbeCache;
    private readonly IDbWorkProbeResponse _dbWorkProbeResponse = dbWorkProbeResponse;

    public async Task ResponseReceived(WorkProbeResponse workProbeResponse)
    {
        await _workProbeCache.TryToRemoveFromCache(workProbeResponse.Id);

        await _dbWorkProbeResponse.SaveWorkProbeResponse(workProbeResponse);

        if (workProbeResponse.Success && workProbeResponse.Type == WorkProbeResponseType.Walk)
        {
            var templates = await _dbWorkProbeResponse.GetItemTemplates(workProbeResponse.Id);
            
            if (templates.Any())
                foreach (var template in templates)
                    await _dbWorkProbeResponse.CreateItemsFromTemplate(template, workProbeResponse);
        }
    }
}
