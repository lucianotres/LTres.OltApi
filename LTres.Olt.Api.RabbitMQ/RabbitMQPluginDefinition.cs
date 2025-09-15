
using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.Plugin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LTres.Olt.Api.RabbitMQ;

public class RabbitMQPluginDefinition : ILTresOltApiPlugin
{
    public string Name => "RabbitMQ";

    public Task AfterStart(IServiceProvider services) => Task.CompletedTask;

    public Task AfterStop(IServiceProvider services) => Task.CompletedTask;

    public Task BeforeStart(IServiceProvider services) => Task.CompletedTask;

    public Task BeforeStop(IServiceProvider services) => Task.CompletedTask;

    public void Configure(IServiceCollection services, IConfiguration configuration) => services
        .Configure<RabbitMQConfiguration>(o => o.FillFromEnvironmentVars())
        .AddTransient<IWorkerDispatcher, RabbitMQWorkExecutionDispatcher>()
        .AddTransient<IWorkerResponseReceiver, RabbitMQWorkResponseReceiver>();
}