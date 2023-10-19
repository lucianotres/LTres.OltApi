using LTres.OltApi.Common;
using LTres.OltApi.Core.Workers;
using LTres.OltApi.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using LTres.OltApi.Snmp;
using LTres.OltApi.Core;

Console.WriteLine("Starting the worker..");

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")   
    .Build();

var serviceController = new ServiceCollection()
    .AddLogging(p => p.AddConfiguration(configuration.GetSection("Logging")).AddConsole())
    .AddSingleton<IWorkerAction, WorkAction>()
    .AddSingleton<IWorker, RabbitMQWorkExecution>()
    .AddSingleton<IWorkerActionPing, WorkPingAction>()
    .AddSingleton<IWorkerActionSnmpGet, WorkSnmpGetAction>()
    .AddSingleton<IWorkerActionSnmpWalk, WorkSnmpWalkAction>()
    .AddSingleton<IWorkProbeCalc, WorkProbeCalcValues>()
    .AddOptions()
    .Configure<RabbitMQConfiguration>(o => o.FillFromEnvironmentVars());

var serviceProvider = serviceController.BuildServiceProvider();
var worker = serviceProvider.GetService<IWorker>();
worker?.Start();

Console.WriteLine("\r\nStarted successfully, press any key to stop.");
Console.Read();

worker?.Stop();
Console.WriteLine("Stopped");