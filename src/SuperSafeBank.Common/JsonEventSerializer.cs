using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Common;

public class JsonEventSerializer : IEventSerializer
{
    private readonly IEnumerable<Assembly> _assemblies;
    private ConcurrentDictionary<string, Type> _typesCache = new();
    
    private static readonly JsonSerializerOptions _serializerOptions = new ()
    {
        IgnoreReadOnlyFields = false,
        IgnoreReadOnlyProperties = false,
        IncludeFields = true,
    };

    private static readonly Newtonsoft.Json.JsonSerializerSettings JsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings()
    {
        ConstructorHandling = Newtonsoft.Json.ConstructorHandling.AllowNonPublicDefaultConstructor,
        ContractResolver = new PrivateSetterContractResolver()
    };

    public JsonEventSerializer(IEnumerable<Assembly> assemblies) =>
        _assemblies = assemblies ?? new[] { Assembly.GetExecutingAssembly() };

    public IDomainEvent<TKey> Deserialize<TKey>(string type, ReadOnlySpan<byte> data)
    {
        Type eventType = ResolveEventType(type);
        // still not great support for deserializing immutable types with System.Text.Json
        var jsonData = System.Text.Encoding.UTF8.GetString(data);
        var result = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonData, eventType, JsonSerializerSettings);
        return (IDomainEvent<TKey>)result;
    }

    public byte[] Serialize<TKey>(IDomainEvent<TKey> @event)
    {
        var json = JsonSerializer.Serialize((dynamic)@event, _serializerOptions);
        var data = Encoding.UTF8.GetBytes(json);
        return data;
    }

    private Type ResolveEventType(string type)
    {
        var eventType = _typesCache.GetOrAdd(type, _ => _assemblies.Select(a => a.GetType(type, false))
                                                                                .FirstOrDefault(t => t != null) ?? Type.GetType(type));
        if (null == eventType)
            throw new ArgumentOutOfRangeException(nameof(type), $"invalid event type: {type}");
        return eventType;
    }
}