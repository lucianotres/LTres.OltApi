using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace LTres.Olt.Api.Mongo;

public class MongoConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}

public static class MongoConfigExtensions
{
    public static IServiceCollection AddMongoConfigToDatabase(this IServiceCollection services)
    {
        services.AddScoped(sp =>
        {
            var config = sp.GetRequiredService<IOptions<MongoConfig>>().Value;
            var client = new MongoClient(config.ConnectionString);
            return client.GetDatabase(config.DatabaseName);
        });

        return services;
    } 
}