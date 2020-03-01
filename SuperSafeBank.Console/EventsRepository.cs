using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Console
{
    public class EventsRepository : IDisposable, IEventsRepository
    {
        private IProducer<Null, string> _producer;

        private readonly string _topicName;
        
        public EventsRepository(string topicName, ProducerConfig producerConfig)
        {
            _topicName = topicName;
         
            var builder = new ProducerBuilder<Null, string>(producerConfig);
            _producer = builder.Build();
        }

        public async Task AppendAsync<TKey>(IEnumerable<IDomainEvent<TKey>> events)
        {
            foreach (var @event in events)
            {
                var serialized = System.Text.Json.JsonSerializer.Serialize(@event);
                var eventType = @event.GetType();

                var headers = new Headers
                {
                    {"aggregate", Encoding.UTF8.GetBytes(@event.AggregateId.ToString())},
                    {"type", Encoding.UTF8.GetBytes(eventType.FullName)}
                };

                var message = new Message<Null, string>() {Value = serialized, Headers = headers};
                await _producer.ProduceAsync(_topicName, message);
            }
        }

        public void Dispose()
        {
            _producer?.Dispose();
            _producer = null;
        }
    }

}