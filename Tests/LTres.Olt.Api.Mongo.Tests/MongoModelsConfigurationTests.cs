using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace LTres.Olt.Api.Mongo.Tests;

public class MongoModelsConfigurationTests
{
    [Fact]
    public void RegisterClassMap_ShouldConfigureMongoDBCorrectly()
    {
        //act
        MongoModelsConfiguration.RegisterClassMap();

        //get settings
        var guidSerializer = BsonSerializer.SerializerRegistry.GetSerializer<Guid>() as GuidSerializer;
#pragma warning disable CS0618
        var guidRepresentationMode = BsonDefaults.GuidRepresentationMode;
#pragma warning restore CS0618
        var bsonClassMap = BsonClassMap.LookupClassMap(typeof(MockObjectToSerializer));
        var memberMap = bsonClassMap?.GetMemberMap("MemberString");

        //assert
        Assert.NotNull(guidSerializer);
        Assert.Equal(GuidRepresentation.Standard, guidSerializer.GuidRepresentation);
        Assert.Equal(GuidRepresentationMode.V3, guidRepresentationMode);
        Assert.NotNull(memberMap);
        Assert.True(memberMap.IgnoreIfNull);
    }

    class MockObjectToSerializer
    {
        public string MemberString { get; set; } = string.Empty;
    }
}
