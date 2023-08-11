using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LTres.OltApi.Common.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace LTres.OltApi.Mongo;

public static class MongoModelsConfiguration
{
    public static void RegisterClassMap()
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        BsonClassMap.RegisterClassMap<OLT_Host>(cm => 
        {
            cm.AutoMap();
            cm.MapIdField(p => p.Id);
        });
    }
}