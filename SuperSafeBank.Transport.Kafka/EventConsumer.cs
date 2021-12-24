using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common.EventBus;
using System.Runtime.Serialization;
using System.Text;

namespace SuperSafeBank.Transport.Kafka
{
    public class EventConsumer : IDisposable, IEventConsumer
    {
        private IConsumer<Guid, string> _consumer;
        private readonly ILogger<EventConsumer> _logger;

        public EventConsumer(EventsConsumerConfig config, ILogger<EventConsumer> logger)
        {
            _logger = logger;

            var consumerConfig = new ConsumerConfig
            {
                GroupId = config.ConsumerGroup,
                BootstrapServers = config.KafkaConnectionString,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnablePartitionEof = true
            };

            var consumerBuilder = new ConsumerBuilder<Guid, string>(consumerConfig);
            var keyDeserializerFactory = new KeyDeserializerFactory();
            consumerBuilder.SetKeyDeserializer(keyDeserializerFactory.Create<Guid>());

            _consumer = consumerBuilder.Build();
            
            _consumer.Subscribe(config.TopicName);
        }

        public Task StartConsumeAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                var topics = string.Join(",", _consumer.Subscription);
                _logger.LogInformation("started Kafka consumer {ConsumerName} on {ConsumerTopic}", _consumer.Name, topics);

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = _consumer.Consume(cancellationToken);
                        if (cr.IsPartitionEOF)
                            continue;
                        
                        var messageTypeHeader = cr.Message.Headers.First(h => h.Key == "type");
                        var eventTypeName = Encoding.UTF8.GetString(messageTypeHeader.GetValueBytes());
                        var eventType = Type.GetType(eventTypeName);
                        var @event = System.Text.Json.JsonSerializer.Deserialize(cr.Message.Value, eventType) as IIntegrationEvent;
                        if(null == @event)
                            throw new SerializationException($"unable to deserialize event {eventTypeName} : {cr.Message.Value}");

                        await OnEventReceived(@event);
                    }
                    catch (OperationCanceledException ex)
                    {
                        _logger.LogWarning(ex, "consumer {ConsumerName} on {ConsumerTopic} was stopped: {StopReason}", _consumer.Name, topics, ex.Message);
                        OnConsumerStopped();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"an exception has occurred while consuming a message: {ex.Message}");
                        OnExceptionThrown(ex);
                    }
                }
            }, cancellationToken);
        }

        public event EventReceivedHandler EventReceived;

        protected virtual Task OnEventReceived(IIntegrationEvent @event)
        {
            var handler = EventReceived;
            return handler?.Invoke(this, @event);
        }
                
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