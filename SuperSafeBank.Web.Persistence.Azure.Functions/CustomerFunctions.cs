using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using SuperSafeBank.Core;

namespace SuperSafeBank.Web.Persistence.Azure.Functions
{
    public class CustomerFunctions : BaseFunction
    {
        public CustomerFunctions(IEventSerializer eventSerializer, IMediator mediator) : base(eventSerializer, mediator)
        {
        }

        [FunctionName("CustomerArchive")]
        public async Task Archive([ServiceBusTrigger("aggregate-customer", "archive", Connection = "AzureWebJobsServiceBus")]Message msg)
        {
            await HandleMessage(msg);
        }

        [FunctionName("CustomerDetails")]
        public async Task Details([ServiceBusTrigger("aggregate-customer", "details", Connection = "AzureWebJobsServiceBus")]Message msg)
        {
            await HandleMessage(msg);
        }
    }
}