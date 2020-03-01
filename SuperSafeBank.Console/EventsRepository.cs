using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Console
{
    public class EventsRepository<TA, TKey> : IDisposable, IEventsRepository<TA, TKey>
        where TA : IAggregateRoot<TKey>
    {
        private IProducer<TKey, string> _producer;

        private readonly string _topicName;
        
        public EventsRepository(string topicBaseName, ProducerConfig producerConfig)
        {
            var aggregateType = typeof(TA);

            _topicName = $"{topicBaseName}-{aggregateType.Name}";
         
            var builder = new ProducerBuilder<TKey, string>(producerConfig);
            builder.SetKeySerializer(new KeySerializer<TKey>());

            _producer = builder.Build();
        }

        public async Task AppendAsync(TA aggregateRoot)
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
                    {"type", Encoding.UTF8.GetBytes(eventType.Name)}
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