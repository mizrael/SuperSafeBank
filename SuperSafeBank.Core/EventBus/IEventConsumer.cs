using System.Threading;
using System.Threading.Tasks;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Core.EventBus
{
    public interface IEventConsumer
    {
        Task ConsumeAsync(CancellationToken stoppingToken);
    }

    public interface IEventConsumer<TA, out TKey> : IEventConsumer where TA : IAggregateRoot<TKey>
    {
        event EventReceivedHandler<TKey> EventReceived;
    }

    public delegate Task EventReceivedHandler<in TKey>(object sender, IDomainEvent<TKey> e);
}