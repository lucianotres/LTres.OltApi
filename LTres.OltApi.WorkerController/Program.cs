using LTres.OltApi.Common;
using LTres.OltApi.Core.Workers;
using LTres.OltApi.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using LTres.OltApi.Mongo;
using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Core;

Console.WriteLine("Starting the worker controller..");

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .Configure<MongoConfig>(o => builder.Configuration.Bind("MongoConfig", o))
    .Configure<RabbitMQConfiguration>(o => o.FillFromEnvironmentVars());

builder.Services
    .AddTransient<IDbWorkProbeInfo, MongoDbWorkProbeInfo>()
    .AddTransient<IDbWorkProbeResponse, MongoDbWorkProbeResponse>()
    .AddTransient<IDbWorkCleanUp, MongoDbWorkCleanUp>()
    .AddTransient<IWorkProbeCache, WorkProbeCache>()
    .AddTransient<IWorkListController, WorkListManager>()
    .AddTransient<IWorkResponseController, WorkDoneManager>()
    .AddTransient<IWorkerDispatcher, RabbitMQWorkExecutionDispatcher>()
    .AddTransient<IWorkerResponseReceiver, RabbitMQWorkResponseReceiver>()
    .AddSingleton<ILogCounter, LogCounter>()
    .AddHostedService<LogCounterPrinter>()
    .AddHostedService<WorkController>();

var app = builder.Build();

//do migrations for database
MongoModelsConfiguration.RegisterClassMap();
await MongoDbOltApiMigrations.Do(app.Services.GetRequiredService<IOptions<MongoConfig>>().Value);

await app.StartAsync();
Console.WriteLine("Started");

await app.WaitForShutdownAsync();
Console.WriteLine("Stopped");