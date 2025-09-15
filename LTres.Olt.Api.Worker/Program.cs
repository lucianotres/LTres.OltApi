using LTres.Olt.Api.Common;
using LTres.Olt.Api.Core.Workers;
using LTres.Olt.Api.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LTres.Olt.Api.Core;
using LTres.Olt.Api.Common.Plugin;
using LTres.Olt.Api.Common.Models;
using LTres.Olt.Api.Core.Tools;

Console.WriteLine($"Starting the worker..");

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPluginManager(builder.Configuration);

if (OltApiConfiguration.Instance.usingMock)
{
    builder.Services
        .AddSingleton(new MockSNMPItems(Path.Combine(AppContext.BaseDirectory, "mock_items.csv")))
        .AddTransient<IWorkerActionSnmpGet, MockSnmpGetAction>()
        .AddTransient<IWorkerActionSnmpWalk, MockSnmpWalkAction>()
        .AddTransient<IWorkerActionPing, MockPingAction>();

    Console.WriteLine("Using Mock SNMP Items");
}
else
    builder.Services.AddTransient<IWorkerActionPing, WorkPingAction>();

builder.Services
    .AddTransient<IWorkerAction, WorkAction>()
    .AddTransient<IWorkProbeCalc, WorkProbeCalc2Values>()
    .AddSingleton<ILogCounter, LogCounter>()
    .AddHostedService<LogCounterPrinter>()
    .AddHostedService<RabbitMQWorkExecution>();

var app = builder.Build();

await app.Services.PluginManagerBeforeStart();
await app.StartAsync();
await app.Services.PluginManagerAfterStart();
Console.WriteLine("Started");

await app.Services.PluginManagerBeforeStop();
await app.WaitForShutdownAsync();
await app.Services.PluginManagerAfterStop();
Console.WriteLine("Stopped");
