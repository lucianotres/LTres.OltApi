using LTres.OltApi.Common;
using LTres.OltApi.Core.Workers;
using LTres.OltApi.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LTres.OltApi.Snmp;
using LTres.OltApi.Core;

Console.WriteLine("Starting the worker..");

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .Configure<RabbitMQConfiguration>(o => o.FillFromEnvironmentVars());

builder.Services
    .AddTransient<IWorkerAction, WorkAction>()
    .AddTransient<IWorkerActionPing, WorkPingAction>()
    .AddTransient<IWorkerActionSnmpGet, WorkSnmpGetAction>()
    .AddTransient<IWorkerActionSnmpWalk, WorkSnmpWalkAction>()
    .AddTransient<IWorkProbeCalc, WorkProbeCalcValues>()
    .AddSingleton<ILogCounter, LogCounter>()
    .AddHostedService<LogCounterPrinter>()
    .AddHostedService<RabbitMQWorkExecution>();

var app = builder.Build();

await app.StartAsync();
Console.WriteLine("Started successfully");

await app.WaitForShutdownAsync();
Console.WriteLine("Stopped");