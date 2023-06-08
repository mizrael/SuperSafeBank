using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common.EventBus;
using System.Text;

namespace SuperSafeBank.Transport.Kafka
{
    public class EventProducer : IDisposable, IEventProducer
    {
        private IProducer<Guid, string> _producer;
        private readonly KafkaProducerConfig _config;
        private readonly ILogger<EventProducer> _logger;

        public EventProducer(KafkaProducerConfig config, ILogger<EventProducer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            var producerConfig = new ProducerConfig { BootstrapServers = config.KafkaConnectionString };
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

            await _producer.ProduceAsync(_config.TopicName, message);
        }

        public void Dispose()
        {
            _producer?.Dispose();
            _producer = null;
        }
    }

}