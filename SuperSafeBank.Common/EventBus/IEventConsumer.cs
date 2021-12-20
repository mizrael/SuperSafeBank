using SuperSafeBank.Common.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Common.EventBus
{
    public interface IEventConsumer
    {
        Task ConsumeAsync(CancellationToken stoppingToken);
    }

    public interface IEventConsumer<TA, out TKey> : IEventConsumer where TA : IAggregateRoot<TKey>
    {
        event EventReceivedHandler<TKey> EventReceived;        
        event ExceptionThrownHandler ExceptionThrown;        
    }

    public delegate Task EventReceivedHandler<in TKey>(object sender, IDomainEvent<TKey> e);
    public delegate void ExceptionThrownHandler(object sender, Exception e);
}