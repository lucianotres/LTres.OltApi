using LTres.Olt.Api.Common.DbServices;
using LTres.Olt.Api.Common.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace LTres.Olt.Api.Mongo;

public class MongoDbWorkProbeResponse : IDbWorkProbeResponse
{
    private IMongoCollection<OLT_Host_Item> OLT_Host_Items;
    private IMongoCollection<OLT_Host_Item_History> OLT_Host_Items_History;

    public MongoDbWorkProbeResponse(IOptions<MongoConfig> options)
    {
        var config = options.Value;
        var client = new MongoClient(config.ConnectionString);
        var database = client.GetDatabase(config.DatabaseName);

        OLT_Host_Items = database.GetCollection<OLT_Host_Item>("olt_host_items");
        OLT_Host_Items_History = database.GetCollection<OLT_Host_Item_History>("olt_host_items_history");
    }

    public async Task SaveWorkProbeResponse(WorkProbeResponse workProbeResponse)
    {
        var interval = workProbeResponse.Request?.Interval;

        var filter = Builders<OLT_Host_Item>.Filter.Eq(f => f.Id, workProbeResponse.Id);
        var update = Builders<OLT_Host_Item>.Update
            .Set(p => p.ProbedSuccess, workProbeResponse.Success)
            .Set(p => p.NextProbe, workProbeResponse.ProbedAt.AddSeconds(interval.GetValueOrDefault(60)));

        if (workProbeResponse.Success)
        {
            update = update
                .Unset(p => p.ProbeFailedMessage)
                .Set(p => p.LastProbed, workProbeResponse.ProbedAt);

            if (workProbeResponse.ValueInt.HasValue)
                update = update.Set(p => p.ProbedValueInt, workProbeResponse.ValueInt);
            else
                update = update.Unset(p => p.ProbedValueInt);

            if (workProbeResponse.ValueUInt.HasValue)
                update = update.Set(p => p.ProbedValueUInt, workProbeResponse.ValueUInt);
            else
                update = update.Unset(p => p.ProbedValueUInt);

            if (workProbeResponse.ValueStr != null)
                update = update.Set(p => p.ProbedValueStr, workProbeResponse.ValueStr);
            else
                update = update.Unset(p => p.ProbedValueStr);
        }
        else
            update = update.Set(p => p.ProbeFailedMessage, workProbeResponse.FailMessage);


        if (workProbeResponse.Type == WorkProbeResponseType.Value)
        {
            if (workProbeResponse.DoHistory && workProbeResponse.Success)
                await OLT_Host_Items_History.InsertOneAsync(OLT_Host_Item_History.From(workProbeResponse));

            await OLT_Host_Items.UpdateOneAsync(filter, update);
        }
        else if (workProbeResponse.Type == WorkProbeResponseType.Walk)
        {
            await OLT_Host_Items.UpdateOneAsync(filter, update);

            if (workProbeResponse.Values != null)
            {
                var oltItemReference = (await OLT_Host_Items.FindAsync(f => f.Id == workProbeResponse.Id)).FirstOrDefault();

                foreach (var v in workProbeResponse.Values)
                    await AddOrUpdateNestedProbedItem(workProbeResponse, v, oltItemReference?.IdOltHost);
            }
        }
    }

    private async Task AddOrUpdateNestedProbedItem(WorkProbeResponse workProbeResponse, WorkProbeResponseVar workProbeResponseVar, Guid? idOltHost)
    {
        var vfilter = Builders<OLT_Host_Item>.Filter.Eq(f => f.IdRelated, workProbeResponse.Id);
        vfilter &= Builders<OLT_Host_Item>.Filter.Eq(f => f.ItemKey, workProbeResponseVar.Key);

        var vupdate = Builders<OLT_Host_Item>.Update
            .Set(p => p.ProbedSuccess, workProbeResponse.Success);

        if (workProbeResponse.Success)
        {
            vupdate = vupdate
                .Unset(p => p.ProbeFailedMessage)
                .Set(p => p.LastProbed, workProbeResponse.ProbedAt);

            if (workProbeResponseVar.ValueInt.HasValue)
                vupdate = vupdate.Set(p => p.ProbedValueInt, workProbeResponseVar.ValueInt);
            else
                vupdate = vupdate.Unset(p => p.ProbedValueInt);

            if (workProbeResponseVar.ValueUInt.HasValue)
                vupdate = vupdate.Set(p => p.ProbedValueUInt, workProbeResponseVar.ValueUInt);
            else
                vupdate = vupdate.Unset(p => p.ProbedValueUInt);

            if (workProbeResponseVar.ValueStr == null)
                vupdate = vupdate.Unset(p => p.ProbedValueStr);
            else
                vupdate = vupdate.Set(p => p.ProbedValueStr, workProbeResponseVar.ValueStr);
        }
        else
            vupdate = vupdate.Set(p => p.ProbeFailedMessage, workProbeResponse.FailMessage);

        var updateResult = await OLT_Host_Items.UpdateOneAsync(vfilter, vupdate);
        if (updateResult.IsAcknowledged && (updateResult.MatchedCount <= 0))
        {
            await OLT_Host_Items.InsertOneAsync(new OLT_Host_Item()
            {
                IdOltHost = idOltHost,
                IdRelated = workProbeResponse.Id,
                ItemKey = workProbeResponseVar.Key,
                LastProbed = workProbeResponse.Success ? workProbeResponse.ProbedAt : null,
                ProbedSuccess = workProbeResponse.Success,
                ProbeFailedMessage = workProbeResponse.FailMessage,
                ProbedValueInt = workProbeResponseVar.ValueInt,
                ProbedValueUInt = workProbeResponseVar.ValueUInt,
                ProbedValueStr = workProbeResponseVar.ValueStr
            });
        }

        if (workProbeResponse.DoHistory && workProbeResponse.Success)
        {
            var pipelineFindHostItemId = PipelineDefinition<OLT_Host_Item, Guid>
                .Create(new IPipelineStageDefinition[] {
                PipelineStageDefinitionBuilder.Match(vfilter),
                PipelineStageDefinitionBuilder.Project<OLT_Host_Item, Guid>(p => p.Id)
                });

            var resultFindHostItemId = (await OLT_Host_Items.AggregateAsync(pipelineFindHostItemId)).FirstOrDefault();
            if (resultFindHostItemId != Guid.Empty)
            {
                var oltHostItemHistory = new OLT_Host_Item_History()
                {
                    IdItem = resultFindHostItemId,
                    At = workProbeResponse.ProbedAt,
                    ValueInt = workProbeResponseVar.ValueInt,
                    ValueUInt = workProbeResponseVar.ValueUInt,
                    ValueStr = workProbeResponseVar.ValueStr,
                };
                await OLT_Host_Items_History.InsertOneAsync(oltHostItemHistory);
            }
        }
    }

    public async Task<IEnumerable<OLT_Host_Item>> GetItemTemplates(Guid from)
    {
        var filter = Builders<OLT_Host_Item>.Filter.Eq(p => p.From, from);
        filter &= Builders<OLT_Host_Item>.Filter.Eq(p => p.Template, true);

        var templateItems = await OLT_Host_Items.FindAsync(filter);
        return await templateItems.ToListAsync();
    }

    public async Task CreateItemsFromTemplate(OLT_Host_Item itemTemplate, WorkProbeResponse workProbeResponse)
    {
        if (workProbeResponse.Values == null)
            return;

        foreach (var workProbeResponseVar in workProbeResponse.Values)
        {
            var newItemKey = workProbeResponseVar.Key;

            var vfilter = Builders<OLT_Host_Item>.Filter.Eq(f => f.IdRelated, itemTemplate.Id);
            vfilter &= Builders<OLT_Host_Item>.Filter.Eq(f => f.ItemKey, newItemKey);

            var vupdate = Builders<OLT_Host_Item>.Update
                .Set(p => p.Action, itemTemplate.Action)
                .Set(p => p.Interval, itemTemplate.Interval)
                .Set(p => p.Active, itemTemplate.Active);

            var updateResult = await OLT_Host_Items.UpdateOneAsync(vfilter, vupdate);
            if (updateResult.IsAcknowledged && (updateResult.MatchedCount <= 0))
            {
                await OLT_Host_Items.InsertOneAsync(new OLT_Host_Item()
                {
                    IdOltHost = itemTemplate.IdOltHost,
                    IdRelated = itemTemplate.Id,
                    Action = itemTemplate.Action,
                    ItemKey = newItemKey,
                    Interval = itemTemplate.Interval,
                    Active = itemTemplate.Active
                });
            }
        }
    }

}
