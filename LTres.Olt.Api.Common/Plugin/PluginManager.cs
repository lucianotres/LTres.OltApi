using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LTres.Olt.Api.Common.Plugin;


public class PluginManager(PluginManagerConfig config) : ILTresOltApiPlugin
{
    private readonly PluginManagerConfig _config = config;
    private readonly IList<ILTresOltApiPlugin> _pluginsList = [];

    public string Name => "_Manager";

    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        foreach (var active in _config.Active)
        {
            var pathPlugin = Path.Combine(appPath, active);
            if (File.Exists(pathPlugin))
            {
                var assembly = Assembly.LoadFile(pathPlugin);

                var pluginType = assembly
                    .GetExportedTypes()
                    .FirstOrDefault(w => w.IsClass && w.GetInterfaces().Any(t => t == typeof(ILTresOltApiPlugin)));

                if (pluginType != null)
                {
                    var configureMethod = pluginType.GetMethod("Configure", BindingFlags.Public | BindingFlags.Static);
                    configureMethod?.Invoke(null, [services, configuration]);

                    _pluginsList.Add((ILTresOltApiPlugin)Activator.CreateInstance(pluginType)!);
                }
            }
        }
    }

    public async Task AfterStart(IServiceProvider services)
    {
        foreach (var plugin in _pluginsList)
            await plugin.AfterStart(services);
    }

    public async Task AfterStop(IServiceProvider services)
    {
        foreach (var plugin in _pluginsList)
            await plugin.AfterStop(services);
    }

    public async Task BeforeStart(IServiceProvider services)
    {
        foreach (var plugin in _pluginsList)
            await plugin.BeforeStart(services);
    }

    public async Task BeforeStop(IServiceProvider services)
    {
        foreach (var plugin in _pluginsList)
            await plugin.BeforeStop(services);
    }

}

public static class PluginManagerConfigExtensions
{
    public static IServiceCollection AddPluginManager(this IServiceCollection services, IConfiguration configuration)
    {
        var pluginManagerConfig = new PluginManagerConfig();
        configuration.Bind("Plugins", pluginManagerConfig);

        var pluginManager = new PluginManager(pluginManagerConfig);
        pluginManager.Configure(services, configuration);

        services.AddSingleton(pluginManagerConfig);
        return services;
    }

    public static async Task<IServiceProvider> PluginManagerBeforeStart(this IServiceProvider serviceProvider)
    {
        await serviceProvider.GetRequiredService<PluginManager>().BeforeStart(serviceProvider);
        return serviceProvider;
    }

    public static async Task<IServiceProvider> PluginManagerAfterStart(this IServiceProvider serviceProvider)
    {
        await serviceProvider.GetRequiredService<PluginManager>().AfterStart(serviceProvider);
        return serviceProvider;
    }

    public static async Task<IServiceProvider> PluginManagerBeforeStop(this IServiceProvider serviceProvider)
    {
        await serviceProvider.GetRequiredService<PluginManager>().BeforeStop(serviceProvider);
        return serviceProvider;
    }

    public static async Task<IServiceProvider> PluginManagerAfterStop(this IServiceProvider serviceProvider)
    {
        await serviceProvider.GetRequiredService<PluginManager>().AfterStop(serviceProvider);
        return serviceProvider;
    }
}