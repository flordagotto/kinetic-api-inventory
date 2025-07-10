using Microsoft.Extensions.Configuration;
using Polly;
using Polly.CircuitBreaker;
using Polly.Wrap;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;

namespace Services.Services
{
    public interface IRabbitMqPublisher
    {
        Task PublishAsync(string message, string queueName, string routingKey);
    }

    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IChannel _channel;

        private readonly AsyncPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly AsyncPolicyWrap _policyWrap;

        private const string ExchangeName = "inventory_exchange";

        public RabbitMqPublisher(IConfiguration configuration)
        {
            _factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"],
                UserName = configuration["RabbitMQ:UserName"],
                Password = configuration["RabbitMQ:Password"]
            };

            EnsureConnectionAsync().Wait();

            _retryPolicy = Policy.Handle<BrokerUnreachableException>()
           .Or<Exception>()
           .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
               (ex, time) => Console.WriteLine($"Retrying after {time.TotalSeconds}s due to: {ex.Message}"));

            // Open the circuit after 2 failures in a row, and waits 15 seconds to re open it
            _circuitBreakerPolicy = Policy.Handle<BrokerUnreachableException>()
                .Or<Exception>()
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(15),
                    onBreak: (ex, breakDelay) =>
                    {
                        Console.WriteLine($"Circuit breaker opened for {breakDelay.TotalSeconds}s due to: {ex.Message}");
                    },
                    onReset: () => Console.WriteLine("Circuit breaker reset."),
                    onHalfOpen: () => Console.WriteLine("Circuit breaker half-open: testing connection."));

            _policyWrap = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy);

            EnsureConnectionAsync().Wait();
        }

        public async Task PublishAsync(string message, string queueName, string routingKey)
        {
            await EnsureConnectionAsync();

            await _policyWrap.ExecuteAsync(async () =>
            {
                var body = Encoding.UTF8.GetBytes(message);

                await _channel.BasicPublishAsync(exchange: ExchangeName,
                                      routingKey: routingKey,
                                      mandatory: true,
                                      basicProperties: new BasicProperties { Persistent = true },
                                      body: body);
            });
        }

        private async Task EnsureConnectionAsync()
        {
            if (_connection != null && _connection.IsOpen)
                return;

            await _policyWrap.ExecuteAsync(async () =>
            {
                _connection?.Dispose();
                _connection = await _factory.CreateConnectionAsync();

                _channel?.Dispose();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Direct, durable: true);

                await CreateQueues();
            });
        }

        private async Task CreateQueues()
        {
            var queues = new[]
               {
                    ("ProductCreated", "product.created"),
                    ("ProductUpdated", "product.updated"),
                    ("ProductDeleted", "product.deleted")
                };

            foreach (var (queueName, routingKey) in queues)
            {
                await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                await _channel.QueueBindAsync(queue: queueName, exchange: ExchangeName, routingKey: routingKey);
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
