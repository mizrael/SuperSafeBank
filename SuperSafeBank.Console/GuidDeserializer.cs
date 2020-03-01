using System;
using Confluent.Kafka;

namespace SuperSafeBank.Console
{
    internal class GuidDeserializer : IDeserializer<Guid>
    {
        public Guid Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            return new Guid(data);
        }
    }
}