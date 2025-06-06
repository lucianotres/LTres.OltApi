﻿using LTres.OltApi.Common;
using LTres.OltApi.Core.Workers;
using LTres.OltApi.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LTres.OltApi.Snmp;
using LTres.OltApi.Core;
using LTres.OltApi.Core.Tools;
using LTres.OltApi.Common.Models;

OltApiConfiguration configuration = new();
configuration.FillFromEnvironmentVars();

Console.WriteLine($"Starting the worker i{configuration.implementationVersion}..");

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddSingleton(configuration)
    .Configure<RabbitMQConfiguration>(o => o.FillFromEnvironmentVars());

builder.Services.AddTransient<IWorkerAction, WorkAction>();

if (configuration.usingMock)
{
    builder.Services
        .AddSingleton<MockSNMPItems>(new MockSNMPItems(Path.Combine(AppContext.BaseDirectory, "mock_items.csv")))
        .AddTransient<MockSnmpGetAction>()
        .AddTransient<MockSnmpWalkAction>()
        .AddTransient<MockPingAction>();

    Console.WriteLine("Using Mock SNMP Items");
}

if (configuration.implementationVersion == 2)
{
    builder.Services
        .AddTransient<IWorkerActionSnmpGet, WorkSnmpGetAction2>()
        .AddTransient<IWorkerActionSnmpWalk, WorkSnmpWalkAction2>();
}
else
{
    builder.Services
        .AddTransient<IWorkerActionSnmpGet, WorkSnmpGetAction>()
        .AddTransient<IWorkerActionSnmpWalk, WorkSnmpWalkAction>();
}

builder.Services
    .AddTransient<IWorkerActionPing, WorkPingAction>();

builder.Services
    .AddTransient<IWorkProbeCalc, WorkProbeCalc2Values>()
    .AddSingleton<ILogCounter, LogCounter>()
    .AddHostedService<LogCounterPrinter>()
    .AddHostedService<RabbitMQWorkExecution>();

var app = builder.Build();

await app.StartAsync();
Console.WriteLine("Started successfully");

await app.WaitForShutdownAsync();
Console.WriteLine("Stopped");