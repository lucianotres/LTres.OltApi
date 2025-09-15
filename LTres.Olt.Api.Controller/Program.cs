using LTres.Olt.Api.Common;
using LTres.Olt.Api.Core.Workers;
using LTres.Olt.Api.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LTres.Olt.Api.Core;
using LTres.Olt.Api.Common.Plugin;

Console.WriteLine("Starting the worker controller..");

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPluginManager(builder.Configuration)
    .Configure<RabbitMQConfiguration>(o => o.FillFromEnvironmentVars());

builder.Services
    .AddTransient<IWorkListController, WorkListManager>()
    .AddTransient<IWorkResponseController, WorkDoneManager>()
    .AddTransient<IWorkerDispatcher, RabbitMQWorkExecutionDispatcher>()
    .AddTransient<IWorkerResponseReceiver, RabbitMQWorkResponseReceiver>()
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