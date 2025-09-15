using System.Reflection;
using LTres.Olt.Api.Common.Models;
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
        AppDomain.CurrentDomain.AssemblyResolve += ResolvePluginAssemblies;

        var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        foreach (var active in _config.Active)
        {
            var pathPlugin = Path.Combine(appPath, active);

            if (File.Exists(pathPlugin))
            {
                var assembly = Assembly.LoadFrom(pathPlugin);

                var pluginType = assembly
                    .GetExportedTypes()
                    .FirstOrDefault(t => t.IsClass && !t.IsAbstract && typeof(ILTresOltApiPlugin).IsAssignableFrom(t));

                if (pluginType != null && Activator.CreateInstance(pluginType) is ILTresOltApiPlugin plugin)
                {
                    plugin.Configure(services, configuration);
                    _pluginsList.Add(plugin);

                    Console.WriteLine($"Plugin {plugin.Name}, v.{assembly.GetName().Version} activated!");
                }
            }
            else
                Console.WriteLine($"Plugin {active} not found!");
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

    /// <summary>
    /// Handles the AssemblyResolve event to load plugin dependencies from the application's base directory.
    /// </summary>
    private Assembly? ResolvePluginAssemblies(object? sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name).Name;
        if (string.IsNullOrEmpty(assemblyName))
            return null;

        var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var assemblyPath = Path.Combine(appPath, $"{assemblyName}.dll");        

        if (File.Exists(assemblyPath))
            return Assembly.LoadFrom(assemblyPath);

        return null;
    }

}

public static class PluginManagerConfigExtensions
{
    public static IServiceCollection AddPluginManager(this IServiceCollection services, IConfiguration configuration)
    {
        OltApiConfiguration.Instance.FillFromEnvironmentVars();

        var pluginManagerConfig = new PluginManagerConfig();
        configuration.Bind("Plugins", pluginManagerConfig);

        var pluginManager = new PluginManager(pluginManagerConfig);
        pluginManager.Configure(services, configuration);

        services.AddSingleton(pluginManager);
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