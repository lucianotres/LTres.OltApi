using LTres.OltApi.Common;
using LTres.OltApi.Core.Workers;
using LTres.OltApi.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using LTres.OltApi.Mongo;
using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Core;

Console.WriteLine("Starting the worker controller..");

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var logCounter = new LogCounter();

var mongoConfig = new MongoConfig();
configuration.Bind("MongoConfig", mongoConfig);

MongoModelsConfiguration.RegisterClassMap();
await MongoDbOltApiMigrations.Do(mongoConfig);

var serviceController = new ServiceCollection()
    .AddLogging(p => p.AddConfiguration(configuration.GetSection("Logging")).AddConsole())
    .AddSingleton<ILogCounter>(logCounter)
    .AddSingleton<IWorkProbeCache, WorkProbeCache>()
    .AddScoped<IDbWorkProbeInfo, MongoDbWorkProbeInfo>()
    .AddScoped<IDbWorkProbeResponse, MongoDbWorkProbeResponse>()
    .AddScoped<IDbWorkCleanUp, MongoDbWorkCleanUp>()
    .AddScoped<IWorkListController, WorkListManager>()
    .AddScoped<IWorkResponseController, WorkDoneManager>()
    .AddScoped<IWorkerDispatcher, RabbitMQWorkExecutionDispatcher>()
    .AddScoped<IWorkerResponseReceiver, RabbitMQWorkResponseReceiver>()
    .AddSingleton<WorkController>()
    .AddOptions()
    .Configure<RabbitMQConfiguration>(o => o.FillFromEnvironmentVars())
    .Configure<MongoConfig>(o => configuration.Bind("MongoConfig", o));

var serviceProvider = serviceController.BuildServiceProvider();

var controller = serviceProvider.GetService<WorkController>();
controller?.Start();

var loggerCounterCancellationToken = new CancellationTokenSource();
_ = logCounter.RunPeriodicNotification(loggerCounterCancellationToken.Token, 60, s => Console.Write(s));

Console.WriteLine("\r\nStarted successfully, press any key to stop.\r\n");
Console.Read();

controller?.Stop();
loggerCounterCancellationToken.Cancel();
Console.WriteLine("Stopped");