using System;
using System.Linq;
using System.Text;
using Confluent.Kafka;
using SuperSafeBank.Core.EventBus;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Console.EventBus
{
    public class EventConsumer<TA, TKey> : IDisposable, IEventConsumer where TA : IAggregateRoot<TKey>
    {
        private IConsumer<TKey, string> _consumer;

        private readonly string _topicName;

        public EventConsumer(string topicBaseName, string kafkaConnString)
        {
            var aggregateType = typeof(TA);

            _topicName = $"{topicBaseName}-{aggregateType.Name}";

            var consumerConfig = new ConsumerConfig
            {
                GroupId = "events-consumer-group",
                BootstrapServers = kafkaConnString,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnablePartitionEof = true
            };

            var consumerBuilder = new ConsumerBuilder<TKey, string>(consumerConfig);
            var keyDeserializerFactory = new KeyDeserializerFactory();
            consumerBuilder.SetKeyDeserializer(keyDeserializerFactory.Create<TKey>());

            _consumer = consumerBuilder.Build();
        }

        public void Consume()
        {
            _consumer.Subscribe(_topicName);

            while (true)
            {
                try
                {
                    var cr = _consumer.Consume();
                    if (cr.IsPartitionEOF)
                        continue;

                    var messageTypeHeader = cr.Headers.First(h => h.Key == "type");
                    var messageType = Encoding.UTF8.GetString(messageTypeHeader.GetValueBytes());

                    System.Console.WriteLine($"Consumed '{messageType}' message: '{cr.Key}' -> '{cr.Value}' at: '{cr.TopicPartitionOffset}'.");
                }
                catch (ConsumeException e)
                {
                    System.Console.WriteLine($"Error occured: {e.Error.Reason}");
                }
            }
        }

        public void Dispose()
        {
            _consumer?.Dispose();
            _consumer = null;
        }
    }
}