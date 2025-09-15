
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LTres.Olt.Api.Common.Plugin;

public interface ILTresOltApiPlugin
{
    string Name { get; }

    void Configure(IServiceCollection services, IConfiguration configuration);

    Task BeforeStart(IServiceProvider services);
    Task AfterStart(IServiceProvider services);
    Task BeforeStop(IServiceProvider services);
    Task AfterStop(IServiceProvider services);
}
