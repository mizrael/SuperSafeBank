using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Core.EventBus
{
    public interface IEventConsumer
    {
        Task ConsumeAsync(CancellationToken cancellationToken);
    }
}