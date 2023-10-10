using LTres.OltApi.Common;
using LTres.OltApi.Core.Workers;
using LTres.OltApi.WorkController;
using LTres.OltApi.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Starting the worker controller..");

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var serviceController = new ServiceCollection()
    .AddLogging(p => p.AddConfiguration(configuration.GetSection("Logging")).AddConsole())
    .AddTransient<IWorkListController, TestWorkList>()
    .AddTransient<IWorkResponseController, TestWorkList>()
    .AddTransient<IWorkerDispatcher, RabbitMQWorkExecutionDispatcher>()
    .AddTransient<IWorkerResponseReceiver, RabbitMQWorkResponseReceiver>()
    .AddSingleton<WorkController>()
    .AddOptions()
    .Configure<RabbitMQConfiguration>(o => o.FillFromEnvironmentVars());

var serviceProvider = serviceController.BuildServiceProvider();

var controller = serviceProvider.GetService<WorkController>();
controller?.Start();

Console.WriteLine("\r\nStarted successfully, press any key to stop.");
Console.Read();

controller?.Stop();
Console.WriteLine("Stopped");