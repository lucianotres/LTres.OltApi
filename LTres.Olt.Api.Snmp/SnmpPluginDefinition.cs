using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.Models;
using LTres.Olt.Api.Common.Plugin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LTres.Olt.Api.Snmp;

public class SnmpPluginDefinition : ILTresOltApiPlugin
{
    public string Name => "SNMP";

    public Task AfterStart(IServiceProvider services) => Task.CompletedTask;

    public Task AfterStop(IServiceProvider services) => Task.CompletedTask;

    public Task BeforeStart(IServiceProvider services) => Task.CompletedTask;

    public Task BeforeStop(IServiceProvider services) => Task.CompletedTask;

    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        if (OltApiConfiguration.Instance.usingMock)
            return;

        if (OltApiConfiguration.Instance.implementationVersion == 2)
        {
            services
                .AddTransient<IWorkerActionSnmpGet, WorkSnmpGetAction2>()
                .AddTransient<IWorkerActionSnmpWalk, WorkSnmpWalkAction2>();
        }
        else
        {
            services
                .AddTransient<IWorkerActionSnmpGet, WorkSnmpGetAction>()
                .AddTransient<IWorkerActionSnmpWalk, WorkSnmpWalkAction>();
        }
    }
        
}
