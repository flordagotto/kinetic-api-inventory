using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;

namespace Services.Services
{
    public interface IRabbitMqPublisher
    {
        Task PublishAsync(string message, string queueName);
    }

    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IChannel _channel;

        public RabbitMqPublisher(IConfiguration configuration)
        {
            _factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"],
                UserName = configuration["RabbitMQ:UserName"],
                Password = configuration["RabbitMQ:Password"]
            };

            EnsureConnectionAsync().Wait();
        }

        private async Task EnsureConnectionAsync()
        {
            if (_connection != null && _connection.IsOpen)
                return;

            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await CreateQueues();
        }

        public async Task PublishAsync(string message, string queueName)
        {
            await EnsureConnectionAsync();

            // aca falta el envio
        }

        private async Task CreateQueues()
        {
            await _channel.QueueDeclareAsync(queue: "ProductCreated", durable: true, exclusive: false, autoDelete: false, arguments: null);
            await _channel.QueueDeclareAsync(queue: "ProductUpdated", durable: true, exclusive: false, autoDelete: false, arguments: null);
            await _channel.QueueDeclareAsync(queue: "ProductDeleted", durable: true, exclusive: false, autoDelete: false, arguments: null);
        }
    }
}
