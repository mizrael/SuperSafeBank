using SuperSafeBank.Common.Models;
using System;

namespace SuperSafeBank.Common;

public interface IEventSerializer
{
    IDomainEvent<TKey> Deserialize<TKey>(string type, ReadOnlySpan<byte> data);
    byte[] Serialize<TKey>(IDomainEvent<TKey> @event);
}