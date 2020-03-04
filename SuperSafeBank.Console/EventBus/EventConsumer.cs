using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            _consumer.Subscribe(_topicName);
        }

        public Task ConsumeAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = _consumer.Consume(cancellationToken);
                        if (cr.IsPartitionEOF)
                            continue;
                        
                        var messageTypeHeader = cr.Headers.First(h => h.Key == "type");
                        var messageType = Encoding.UTF8.GetString(messageTypeHeader.GetValueBytes());

                        System.Console.WriteLine(
                            $"Consumed '{messageType}' message: '{cr.Key}' -> '{cr.Value}' at: '{cr.TopicPartitionOffset}'.");
                    }
                    catch (OperationCanceledException ce)
                    {
                        System.Console.WriteLine("shutting down consumer...");
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine($"Error occured: {e}");
                    }
                }
            }, cancellationToken);
        }

        public void Dispose()
        {
            _consumer?.Dispose();
            _consumer = null;
        }
    }
}