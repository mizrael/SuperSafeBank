using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Core.EventBus;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Transport.Kafka
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddKafka(this IServiceCollection services, EventConsumerConfig consumerConfig)
        {
            return services.AddSingleton(consumerConfig)
                .AddSingleton(typeof(IEventConsumer<,>), typeof(EventConsumer<,>))
                .AddKafkaEventProducer<Customer, Guid>(consumerConfig)
                .AddKafkaEventProducer<Account, Guid>(consumerConfig);
        }

        private static IServiceCollection AddKafkaEventProducer<TA, TK>(this IServiceCollection services, EventConsumerConfig configuration)
            where TA : class, IAggregateRoot<TK>
        {
            return services.AddSingleton<IEventProducer<TA, TK>>(ctx =>
            {
                var logger = ctx.GetRequiredService<ILogger<EventProducer<TA, TK>>>();
                return new EventProducer<TA, TK>(configuration.TopicBaseName, configuration.KafkaConnectionString, logger);
            });
        }
    }
}