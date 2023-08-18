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
        //register GUID standard representation at MongoDb
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        #pragma warning disable CS0618
        BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
        BsonDefaults.GuidRepresentationMode = GuidRepresentationMode.V3;
        #pragma warning restore CS0618

        //define the class Map for our models
        BsonClassMap.RegisterClassMap<OLT_Host>(cm => 
        {
            cm.AutoMap();
            cm.MapIdField(p => p.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
        });
    }
}