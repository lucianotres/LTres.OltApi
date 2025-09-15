using MongoDB.Driver;
using Moq;

namespace LTres.Olt.Api.Mongo.Tests.Utils;

public static class MockCommons
{
    public static void SetupIAsyncCursor<T>(this Mock<IAsyncCursor<T>> mockCursor, IList<T> fakeList) where T : class
    {
        mockCursor.Setup(x => x.Current).Returns(fakeList);
        mockCursor
            .SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor
            .SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Returns(Task.FromResult(false));
    }

    public static void SetupCollection<T>(this Mock<IMongoCollection<T>> mockCollection, Mock<IAsyncCursor<T>> mockCursor) where T : class
    {
        mockCollection
            .Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<T>>(),
                It.IsAny<FindOptions<T, T>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);
    }

    public static void SetupDatabase<T>(this Mock<IMongoDatabase> mockDatabase, string collectionName, Mock<IMongoCollection<T>> mockCollection) where T : class
    {
        mockDatabase
            .Setup(m => m.GetCollection<T>(collectionName, null))
            .Returns(mockCollection.Object);
    }
}