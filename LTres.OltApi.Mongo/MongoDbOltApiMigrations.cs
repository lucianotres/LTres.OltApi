using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Amazon.Runtime.SharedInterfaces;
using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Common.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Linq;

namespace LTres.OltApi.Mongo;

public static class MongoDbOltApiMigrations
{

    public static async Task Do(MongoConfig config)
    {
        var client = new MongoClient(config.ConnectionString);
        var database = client.GetDatabase(config.DatabaseName);

        await CheckOrCreateCollections(database);
        await CheckOrCreateIndexes(database);
    }

    private static async Task CheckOrCreateCollections(IMongoDatabase database)
    {
        var collections = (await database.ListCollectionNamesAsync()).ToList();

        if (!collections.Any(s => s == "olt_hosts"))
            await database.CreateCollectionAsync("olt_hosts");

        if (!collections.Any(s => s == "olt_host_items"))
            await database.CreateCollectionAsync("olt_host_items");

        if (!collections.Any(s => s == "olt_host_items_history"))
            await database.CreateCollectionAsync("olt_host_items_history");

    }

    private static async Task CheckOrCreateIndexes(IMongoDatabase database)
    {
        var collectionOltHostItems = database.GetCollection<OLT_Host_Item>("olt_host_items");
        var indexesOltHostItems = (await collectionOltHostItems.Indexes.ListAsync()).ToList();
        if (!indexesOltHostItems.Any(i => i.FirstOrDefault(f => f.Name == "name").Value.AsString == "idx_IdOltHost"))
        {
            var keys = Builders<OLT_Host_Item>.IndexKeys
                .Ascending(p => p.IdOltHost)
                .Ascending(p => p.ItemKey);
            var createIndexModel = new CreateIndexModel<OLT_Host_Item>(keys, new CreateIndexOptions() {  Name = "idx_IdOltHost"});

            await collectionOltHostItems.Indexes.CreateOneAsync(createIndexModel);
        }
        if (!indexesOltHostItems.Any(i => i.FirstOrDefault(f => f.Name == "name").Value.AsString == "idx_IdRelated"))
        {
            var keys = Builders<OLT_Host_Item>.IndexKeys
                .Ascending(p => p.IdRelated)
                .Ascending(p => p.ItemKey);
            var createIndexModel = new CreateIndexModel<OLT_Host_Item>(keys, new CreateIndexOptions() {  Name = "idx_IdRelated"});

            await collectionOltHostItems.Indexes.CreateOneAsync(createIndexModel);
        }

        var collectionOltHostItemsHistory = database.GetCollection<OLT_Host_Item_History>("olt_host_items_history");
        var indexesOltHostItemsHistory = (await collectionOltHostItemsHistory.Indexes.ListAsync()).ToList();
        if (!indexesOltHostItemsHistory.Any(i => i.FirstOrDefault(f => f.Name == "name").Value.AsString == "idx_IdItem"))
        {
            var keys = Builders<OLT_Host_Item_History>.IndexKeys.Ascending(p => p.IdItem).Ascending(p => p.At);
            var createIndexModel = new CreateIndexModel<OLT_Host_Item_History>(keys, new CreateIndexOptions() {  Name = "idx_IdItem"});

            await collectionOltHostItemsHistory.Indexes.CreateOneAsync(createIndexModel);
        }
    }
}