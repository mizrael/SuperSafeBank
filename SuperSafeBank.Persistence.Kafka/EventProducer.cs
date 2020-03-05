using System;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using SuperSafeBank.Core.EventBus;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Persistence.Kafka
{
    public class EventProducer<TA, TKey> : IDisposable, IEventProducer<TA, TKey>
        where TA : IAggregateRoot<TKey>
    {
        private IProducer<TKey, string> _producer;

        private readonly string _topicName;
        
        public EventProducer(string topicBaseName, string kafkaConnString)
        {
            var aggregateType = typeof(TA);

            _topicName = $"{topicBaseName}-{aggregateType.Name}";

            var producerConfig = new ProducerConfig { BootstrapServers = kafkaConnString };
            var producerBuilder = new ProducerBuilder<TKey, string>(producerConfig);
            producerBuilder.SetKeySerializer(new KeySerializer<TKey>());
            _producer = producerBuilder.Build();
        }

        public async Task DispatchAsync(TA aggregateRoot)
        {
            if(null == aggregateRoot)
                throw new ArgumentNullException(nameof(aggregateRoot));

            foreach (var @event in aggregateRoot.Events)
            {
                var eventType = @event.GetType(); 
                
                var serialized = System.Text.Json.JsonSerializer.Serialize(@event, eventType);
                
                var headers = new Headers
                {
                    {"aggregate", Encoding.UTF8.GetBytes(@event.AggregateId.ToString())},
                    {"type", Encoding.UTF8.GetBytes(eventType.AssemblyQualifiedName)}
                };

                var message = new Message<TKey, string>()
                {
                    Key = @event.AggregateId,
                    Value = serialized, 
                    Headers = headers
                };
                
                await _producer.ProduceAsync(_topicName, message);
            }

            aggregateRoot.ClearEvents();
        }

        public void Dispose()
        {
            _producer?.Dispose();
            _producer = null;
        }
    }

}