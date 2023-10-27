using LTres.OltApi.Common;
using LTres.OltApi.Core.Workers;
using LTres.OltApi.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LTres.OltApi.Snmp;
using LTres.OltApi.Core;

var implementationInt = 2;
{
    var implementationStr = Environment.GetEnvironmentVariable("LTRES_SNMP_IMPLEMENTATION");

    if (!string.IsNullOrWhiteSpace(implementationStr) && int.TryParse(implementationStr, out int i) && i > 0 && i <=2)
        implementationInt = i;
}

Console.WriteLine($"Starting the worker i{implementationInt}..");

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .Configure<RabbitMQConfiguration>(o => o.FillFromEnvironmentVars());

builder.Services.AddTransient<IWorkerAction, WorkAction>();

if (implementationInt == 2)
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
    .AddTransient<IWorkerActionPing, WorkPingAction>()
    .AddTransient<IWorkProbeCalc, WorkProbeCalcValues>()
    .AddSingleton<ILogCounter, LogCounter>()
    .AddHostedService<LogCounterPrinter>()
    .AddHostedService<RabbitMQWorkExecution>();

var app = builder.Build();

await app.StartAsync();
Console.WriteLine("Started successfully");

await app.WaitForShutdownAsync();
Console.WriteLine("Stopped");