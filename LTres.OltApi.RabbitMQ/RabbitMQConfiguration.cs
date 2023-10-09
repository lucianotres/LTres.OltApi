namespace LTres.OltApi.RabbitMQ;

public class RabbitMQConfiguration
{
    public string HostName { get; set; } = "localhost";

    public string QueueName_do { get; set; } = "ltres_worker_do";
    public string QueueName_done { get; set; } = "ltres_worker_done";
}

public static class RabbitMQConfigurationExtensions
{
    public static RabbitMQConfiguration FillFromEnvironmentVars(this RabbitMQConfiguration configuration)
    {
        var env_hostname = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
        if (!string.IsNullOrWhiteSpace(env_hostname))
            configuration.HostName = env_hostname;

        return configuration;
    }
}