using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using SuperSafeBank.Core;
using SuperSafeBank.Core.EventBus;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Persistence.Kafka
{
    public class EventConsumer<TA, TKey> : IDisposable, IEventConsumer where TA : IAggregateRoot<TKey>
    {
        private IConsumer<TKey, string> _consumer;
        private readonly IEventDeserializer _eventDeserializer;

        public EventConsumer(string topicBaseName, string kafkaConnString, IEventDeserializer eventDeserializer)
        {
            _eventDeserializer = eventDeserializer;
            var aggregateType = typeof(TA);

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
            
            var topicName = $"{topicBaseName}-{aggregateType.Name}";
            _consumer.Subscribe(topicName);
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
                        var eventType = Encoding.UTF8.GetString(messageTypeHeader.GetValueBytes());
                        
                        var @event = _eventDeserializer.Deserialize<TKey>(eventType, cr.Value);
                        if(null == @event)
                            throw new SerializationException($"unable to deserialize event {eventType} : {cr.Value}");
                        OnEventReceived(@event);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("shutting down consumer...");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error occured: {e}");
                    }
                }
            }, cancellationToken);
        }

        public delegate void EventReceivedHandler(object sender, IDomainEvent<TKey> e);
        public event EventReceivedHandler EventReceived;
        protected virtual void OnEventReceived(IDomainEvent<TKey> e)
        {
            var handler = EventReceived;
            handler?.Invoke(this, e);
        }

        public void Dispose()
        {
            _consumer?.Dispose();
            _consumer = null;
        }
    }
}