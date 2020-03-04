using System;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace SuperSafeBank.Persistence.Kafka
{
    internal class KeySerializer<TKey> : ISerializer<TKey>
    {
        public byte[] Serialize(TKey data, SerializationContext context)
        {
            if(data is Guid g)
                return g.ToByteArray();
            var json = JsonSerializer.Serialize(data);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}