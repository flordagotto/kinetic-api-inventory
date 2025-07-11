using DTOs.RabbitDtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Wrap;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Services.Services
{
    public interface IRabbitMqPublisher
    {
        Task PublishAsync(EventMessage productMessage);
    }

    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private const string PRODUCT_CREATED_ROUTING_KEY = "product.created";
        private const string PRODUCT_UPDATED_ROUTING_KEY = "product.updated";
        private const string PRODUCT_DELETED_ROUTING_KEY = "product.deleted";

        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IChannel _channel;

        private readonly JsonSerializerOptions _options;

        private readonly AsyncPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly AsyncPolicyWrap _policyWrap;

        private const string EXCHANGE_NAME = "inventory_exchange";

        public RabbitMqPublisher(IConfiguration configuration)
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };

            _factory = new ConnectionFactory
            {
                //HostName = configuration["RabbitMQ:HostName"],
                //Port = int.Parse(configuration["RabbitMQ:Port"]),
                HostName = "localhost",
                Port = 5672,
                UserName = configuration["RabbitMQ:UserName"],
                Password = configuration["RabbitMQ:Password"]
            };

            _retryPolicy = Policy.Handle<BrokerUnreachableException>()
           .Or<Exception>()
           .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
               (ex, time) => Console.WriteLine($"Retrying after {time.TotalSeconds}s due to: {ex.Message}"));

            // Opens the circuit after 2 failures in a row, and waits 15 seconds to re open it
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
        }

        public async Task PublishAsync(EventMessage productMessage)
        {
            await EnsureConnectionAsync();

            var routingKey = GetRoutingKey(productMessage.EventType);

            var message = JsonSerializer.Serialize(productMessage, productMessage.GetType(), _options);

            await _policyWrap.ExecuteAsync(async () =>
            {
                var body = Encoding.UTF8.GetBytes(message);

                await _channel.BasicPublishAsync(exchange: EXCHANGE_NAME,
                                      routingKey: routingKey,
                                      mandatory: true,
                                      basicProperties: new BasicProperties { Persistent = true },
                                      body: body);
            });
        }

        private string GetRoutingKey(ProductEventType eventType)
        {
            if (eventType is ProductEventType.Created)
                return PRODUCT_CREATED_ROUTING_KEY;

            if (eventType is ProductEventType.Updated)
                return PRODUCT_UPDATED_ROUTING_KEY;

            return PRODUCT_DELETED_ROUTING_KEY;
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

                await _channel.ExchangeDeclareAsync(exchange: EXCHANGE_NAME, type: ExchangeType.Direct, durable: true);

                await CreateQueues();
            });
        }

        private async Task CreateQueues()
        {
            var queues = new[]
               {
                    ("ProductCreated", PRODUCT_CREATED_ROUTING_KEY),
                    ("ProductUpdated", PRODUCT_UPDATED_ROUTING_KEY),
                    ("ProductDeleted", PRODUCT_DELETED_ROUTING_KEY)
                };

            foreach (var (queueName, routingKey) in queues)
            {
                await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                await _channel.QueueBindAsync(queue: queueName, exchange: EXCHANGE_NAME, routingKey: routingKey);
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
