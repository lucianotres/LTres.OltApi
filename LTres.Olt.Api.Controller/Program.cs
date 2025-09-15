using LTres.Olt.Api.Common;
using LTres.Olt.Api.Core.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LTres.Olt.Api.Core;
using LTres.Olt.Api.Common.Plugin;

Console.WriteLine("Starting the worker controller..");

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPluginManager(builder.Configuration);

builder.Services
    .AddTransient<IWorkListController, WorkListManager>()
    .AddTransient<IWorkResponseController, WorkDoneManager>()
    .AddSingleton<ILogCounter, LogCounter>()
    .AddSingleton<IWorkProbeCache, WorkProbeCache>()
    .AddHostedService<LogCounterPrinter>()
    .AddHostedService<WorkController>();

var app = builder.Build();

await app.Services.PluginManagerBeforeStart();
await app.StartAsync();
await app.Services.PluginManagerAfterStart();
Console.WriteLine("Started");

await app.Services.PluginManagerBeforeStop();
await app.WaitForShutdownAsync();
await app.Services.PluginManagerAfterStop();
Console.WriteLine("Stopped");