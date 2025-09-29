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
    .AddSingleton<ILogCounter, LogCounter>()
    .AddPluginManager(builder.Configuration);

if (OltApiConfiguration.Instance.usingMock)
{
    builder.Services
        .AddSingleton(new MockSNMPItems(Path.Combine(AppContext.BaseDirectory, "mock_items.csv")))
        .AddTransient<MockSnmpGetAction>()
        .AddTransient<MockSnmpWalkAction>()
        .AddTransient<MockPingAction>();

    Console.WriteLine("Using Mock SNMP Items");
}

builder.Services
    .AddTransient<IWorkerActionPing, WorkPingAction>()
    .AddTransient<IWorkerAction, WorkAction>()
    .AddTransient<IWorkProbeCalc, WorkProbeCalc2Values>()   
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
