using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Console
{
    public interface IEventsRepository<in TA, in TKey>
        where TA : IAggregateRoot<TKey>
    {
        Task AppendAsync(TA aggregateRoot);
    }
}