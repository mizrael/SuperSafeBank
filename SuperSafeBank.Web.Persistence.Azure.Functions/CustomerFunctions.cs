using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Core;

namespace SuperSafeBank.Web.Persistence.Azure.Functions
{
    public class CustomerFunctions
    {
        private readonly IEventSerializer _eventSerializer;

        public CustomerFunctions(IEventSerializer eventSerializer)
        {
            _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        }

        [FunctionName("Archive")]
        public async Task Run([ServiceBusTrigger("aggregate-customer", "archive", Connection = "AzureWebJobsServiceBus")]Message msg, ILogger log)
        {
            log.LogInformation($"C# ServiceBus topic trigger function processed message: {msg}");

            var eventType = msg.UserProperties["type"] as string;
            var @event = _eventSerializer.Deserialize<Guid>(eventType, msg.Body);
            if (null == @event)
                throw new SerializationException($"unable to deserialize event {eventType} : {msg.Body}");

          
        }
    }
}
