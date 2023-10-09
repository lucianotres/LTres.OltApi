using LTres.OltApi.Common;
using LTres.OltApi.Core.Workers;
using LTres.OltApi.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Starting the worker..");

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")   
    .Build();

var serviceController = new ServiceCollection()
    .AddLogging(p => p.AddConfiguration(configuration.GetSection("Logging")).AddConsole())
    .AddSingleton<IWorkerAction, WorkAction>()
    .AddSingleton<IWorker, RabbitMQWorkExecution>()
    .AddOptions()
    .Configure<RabbitMQConfiguration>(o => o.FillFromEnvironmentVars());

var serviceProvider = serviceController.BuildServiceProvider();
var worker = serviceProvider.GetService<IWorker>();
worker?.Start();

Console.WriteLine("\r\nStarted successfully, press any key to stop.");
Console.Read();

worker?.Stop();
Console.WriteLine("Stopped");