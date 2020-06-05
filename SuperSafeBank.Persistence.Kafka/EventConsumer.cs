using System;
using System.Linq;
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

        public EventConsumer(IEventDeserializer eventDeserializer, EventConsumerConfig config)
        {
            _eventDeserializer = eventDeserializer;

            var aggregateType = typeof(TA);

            var consumerConfig = new ConsumerConfig
            {
                GroupId = config.ConsumerGroup,
                BootstrapServers = config.KafkaConnectionString,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnablePartitionEof = true
            };

            var consumerBuilder = new ConsumerBuilder<TKey, string>(consumerConfig);
            var keyDeserializerFactory = new KeyDeserializerFactory();
            consumerBuilder.SetKeyDeserializer(keyDeserializerFactory.Create<TKey>());

            _consumer = consumerBuilder.Build();
            
            var topicName = $"{config.TopicBaseName}-{aggregateType.Name}";
            _consumer.Subscribe(topicName);
        }

        public Task ConsumeAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = _consumer.Consume(stoppingToken);
                        if (cr.IsPartitionEOF)
                            continue;
                        
                        var messageTypeHeader = cr.Headers.First(h => h.Key == "type");
                        var eventType = Encoding.UTF8.GetString(messageTypeHeader.GetValueBytes());
                        
                        var @event = _eventDeserializer.Deserialize<TKey>(eventType, cr.Value);
                        if(null == @event)
                            throw new SerializationException($"unable to deserialize event {eventType} : {cr.Value}");

                        await OnEventReceived(@event);
                    }
                    catch (OperationCanceledException)
                    {
                        OnConsumerStopped();
                    }
                    catch (Exception e)
                    {
                        OnExceptionThrown(e);
                    }
                }
            }, stoppingToken);
        }

        public delegate Task EventReceivedHandler(object sender, IDomainEvent<TKey> e);
        public event EventReceivedHandler EventReceived;
        protected virtual Task OnEventReceived(IDomainEvent<TKey> e)
        {
            var handler = EventReceived;
            return handler?.Invoke(this, e);
        }

        public delegate void ExceptionThrownHandler(object sender, Exception e);
        public event ExceptionThrownHandler ExceptionThrown;
        protected virtual void OnExceptionThrown(Exception e)
        {
            var handler = ExceptionThrown;
            handler?.Invoke(this, e);
        }

        public delegate void ConsumerStoppedHandler(object sender);
        public event ConsumerStoppedHandler ConsumerStopped;
        protected virtual void OnConsumerStopped()
        {
            var handler = ConsumerStopped;
            handler?.Invoke(this);
        }

        public void Dispose()
        {
            _consumer?.Dispose();
            _consumer = null;
        }
    }
}