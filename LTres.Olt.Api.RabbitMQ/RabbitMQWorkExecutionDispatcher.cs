using System.Text;
using System.Text.Json;
using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace LTres.Olt.Api.RabbitMQ;

public class RabbitMQWorkExecutionDispatcher : IWorkerDispatcher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMQConfiguration _configuration;

    public RabbitMQWorkExecutionDispatcher(IOptions<RabbitMQConfiguration> configuration)
    {
        _configuration = configuration.Value;

        var connFactory = new ConnectionFactory()
        {
            HostName = _configuration.HostName
        };

        _connection = connFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(_configuration.QueueName_do, true, false, false, null);
    }

    public void Dispatch(WorkProbeInfo workInfo)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(workInfo));
        _channel.BasicPublish(string.Empty, _configuration.QueueName_do, false, null, body);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
