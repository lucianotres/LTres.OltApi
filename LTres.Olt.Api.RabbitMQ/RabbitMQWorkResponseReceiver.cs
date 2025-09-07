using System.Text;
using System.Text.Json;
using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LTres.Olt.Api.RabbitMQ;

public class RabbitMQWorkResponseReceiver : IWorkerResponseReceiver, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger _log;
    private readonly RabbitMQConfiguration _configuration;

    public event EventHandler<WorkerResponseReceivedEventArgs>? OnResponseReceived;

    public RabbitMQWorkResponseReceiver(
        ILogger<RabbitMQWorkResponseReceiver> logger,
        IOptions<RabbitMQConfiguration> configuration)
    {
        _log = logger;
        _configuration = configuration.Value;

        var connFactory = new ConnectionFactory()
        {
            HostName = _configuration.HostName
        };

        _connection = connFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(_configuration.QueueName_done, true, false, false, null);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += MQReceived;

        _channel.BasicConsume(queue: _configuration.QueueName_done, autoAck: false, consumer: consumer);
    }

    private void MQReceived(object? sender, BasicDeliverEventArgs e)
    {
        _log.LogDebug("Response message received, reading..");
        byte[] body = e.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var workProbeResponse = JsonSerializer.Deserialize<WorkProbeResponse>(message);
        _channel.BasicAck(e.DeliveryTag, false);

        if (workProbeResponse == null)
            _log.LogDebug("No response message read to save.");
        else
        {
            _log.LogDebug("Response message read, starting to save data...");
            OnResponseReceived?.Invoke(this, new WorkerResponseReceivedEventArgs() 
            {
                ProbeResponse = workProbeResponse
            });

            _log.LogDebug("Response saved.");
        }
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
