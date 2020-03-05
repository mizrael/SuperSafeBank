using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Core
{
    public interface IEventDeserializer
    {
        IDomainEvent<TKey> Deserialize<TKey>(string type, byte[] data);
        IDomainEvent<TKey> Deserialize<TKey>(string type, string data);
    }
}