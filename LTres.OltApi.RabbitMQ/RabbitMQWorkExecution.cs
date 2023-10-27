using System.Text;
using System.Text.Json;
using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace LTres.OltApi.RabbitMQ;

public class RabbitMQWorkExecution : IHostedService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _log;
    private readonly RabbitMQConfiguration _configuration;
    private readonly ConcurrentQueue<WorkProbeInfo> _queue;
    private Task? QueueWorkActionExecutionTask;

    public RabbitMQWorkExecution(ILogger<RabbitMQWorkExecution> logger,
        ILogCounter logCounter,
        IServiceProvider serviceProvider,
        IOptions<RabbitMQConfiguration> configuration)
    {
        _log = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration.Value;
        _queue = new ConcurrentQueue<WorkProbeInfo>();

        logCounter.RegisterHookOnPrintResetAction<RabbitMQWorkExecution>(HookOnPrintResetAction);

        var connFactory = new ConnectionFactory()
        {
            HostName = _configuration.HostName
        };

        _connection = connFactory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
    }

    private void HookOnPrintResetAction(ILogCounter counter)
    {
        var queueCount = _queue.Count;
        if (queueCount > 0)
            counter.AddCount("received queue", queueCount);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _log.LogDebug("Starting RabbitMQ work execution..");
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += MQReceived;

        _channel.BasicConsume(queue: _configuration.QueueName_do, autoAck: false, consumer: consumer);
        _log.LogDebug("RabbitMQ work execution started.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _log.LogDebug("RabbitMQ work execution stopped.");
        return Task.CompletedTask;
    }

    private void MQReceived(object? sender, BasicDeliverEventArgs e)
    {
        _log.LogDebug("Message received, reading..");
        byte[] body = e.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var workProbeInfo = JsonSerializer.Deserialize<WorkProbeInfo>(message);
        _channel.BasicAck(e.DeliveryTag, false);

        if (workProbeInfo == null)
            _log.LogDebug("No message read to execute.");
        else
        {
            _queue.Enqueue(workProbeInfo);
            StartQueueWorkActionExecution();
        }
    }

    private void StartQueueWorkActionExecution()
    {
        if (QueueWorkActionExecutionTask != null && !QueueWorkActionExecutionTask.IsCompleted)
            return;

        QueueWorkActionExecutionTask = Task.Run(async () =>
        {
            while (_queue.TryDequeue(out var workProbeInfo))
                await WorkActionExecute(workProbeInfo);
        });
    }

    private async Task WorkActionExecute(WorkProbeInfo workProbeInfo)
    {
        try
        {
            _log.LogDebug("Starting action execution..");
            var workerAction = _serviceProvider.GetRequiredService<IWorkerAction>();

            var cancellationToken = new CancellationTokenSource();
            var workTask = workerAction.Execute(workProbeInfo, cancellationToken.Token);

            if (await Task.WhenAny(workTask, Task.Delay(30000)) == workTask)
            {
                var workProbeResponse = workTask.Result;

                _log.LogDebug("Action executed, sending response..");
                MQResponse(workProbeResponse);
                _log.LogDebug("Response sent.");
            }
            else
            {
                cancellationToken.Cancel();

                MQResponse(new WorkProbeResponse()
                {
                    Id = workProbeInfo.Id,
                    ProbedAt = DateTime.Now,
                    Success = false,
                    FailMessage = "Probing timeouted"
                });
                _log.LogDebug("Timeout response sent.");
            }
        }
        catch (Exception err)
        {
            _log.LogError(err.ToString());
        }
    }

    private void MQResponse(WorkProbeResponse workProbeResponse)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(workProbeResponse));
        _channel.BasicPublish(string.Empty, _configuration.QueueName_done, false, null, body);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}