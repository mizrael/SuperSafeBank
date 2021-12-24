using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common.EventBus;
using System.Text;

namespace SuperSafeBank.Transport.Kafka
{
    public class EventProducer : IDisposable, IEventProducer
    {
        private IProducer<Guid, string> _producer;
        private readonly string _topicName;
        private readonly ILogger<EventProducer> _logger;

        public EventProducer(string topicName, string kafkaConnString, ILogger<EventProducer> logger)
        {
            if (string.IsNullOrWhiteSpace(topicName))            
                throw new ArgumentException($"'{nameof(topicName)}' cannot be null or whitespace.", nameof(topicName));
            
            if (string.IsNullOrWhiteSpace(kafkaConnString))            
                throw new ArgumentException($"'{nameof(kafkaConnString)}' cannot be null or whitespace.", nameof(kafkaConnString));
            
            _logger = logger;
            _topicName = topicName;

            var producerConfig = new ProducerConfig { BootstrapServers = kafkaConnString };
            var producerBuilder = new ProducerBuilder<Guid, string>(producerConfig);
            producerBuilder.SetKeySerializer(new KeySerializer<Guid>());
            _producer = producerBuilder.Build();
        }

        public async Task DispatchAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            if (null == @event)
                throw new ArgumentNullException(nameof(@event));

            _logger.LogInformation("publishing event {EventId} ...", @event.Id);
            var eventType = @event.GetType();

            var serialized = System.Text.Json.JsonSerializer.Serialize(@event, eventType);

            var headers = new Headers
            {
                {"id", Encoding.UTF8.GetBytes(@event.Id.ToString())},
                {"type", Encoding.UTF8.GetBytes(eventType.AssemblyQualifiedName)}
            };

            var message = new Message<Guid, string>()
            {
                Key = @event.Id,
                Value = serialized,
                Headers = headers
            };

            await _producer.ProduceAsync(_topicName, message);
        }

        public void Dispose()
        {
            _producer?.Dispose();
            _producer = null;
        }
    }

}