using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Console
{
    public interface IEventsRepository
    {
        Task AppendAsync<TKey>(IEnumerable<IDomainEvent<TKey>> events);
    }
}