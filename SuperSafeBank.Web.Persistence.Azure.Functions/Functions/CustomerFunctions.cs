using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Azure.WebJobs;
using SuperSafeBank.Core;

namespace SuperSafeBank.Web.Persistence.Azure.Functions.Functions
{
    public class CustomerFunctions : BaseFunction
    {
        public CustomerFunctions(IEventSerializer eventSerializer, IMediator mediator) : base(eventSerializer, mediator)
        {
        }

        [FunctionName(nameof(CustomerCreated))]
        public async Task CustomerCreated([ServiceBusTrigger("aggregate-customer", "created", Connection = "AzureWebJobsServiceBus")] ServiceBusMessage msg)
        {
            await HandleMessage(msg);
        }
    }
}