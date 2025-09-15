using LTres.Olt.Api.Common.DbServices;
using LTres.Olt.Api.Common.Plugin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LTres.Olt.Api.Mongo;

public class MongoDbPluginDefinition : ILTresOltApiPlugin
{
    public string Name => "MongoDB";

    public Task AfterStart(IServiceProvider services) => Task.CompletedTask;

    public Task AfterStop(IServiceProvider services) => Task.CompletedTask;

    public async Task BeforeStart(IServiceProvider services)
    {
        MongoModelsConfiguration.RegisterClassMap();
        await MongoDbOltApiMigrations.Do(services);
    }

    public Task BeforeStop(IServiceProvider services) => Task.CompletedTask;

    public void Configure(IServiceCollection services, IConfiguration configuration) => services
        .Configure<MongoConfig>(o => configuration.Bind("MongoConfig", o))
        .AddMongoConfigToDatabase()
        .AddTransient<IDbWorkProbeInfo, MongoDbWorkProbeInfo>()
        .AddTransient<IDbWorkProbeResponse, MongoDbWorkProbeResponse>()
        .AddTransient<IDbWorkCleanUp, MongoDbWorkCleanUp>();
}
