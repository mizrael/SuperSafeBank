using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Azure.WebJobs;
using SuperSafeBank.Core;

namespace SuperSafeBank.Web.Persistence.Azure.Functions.Functions
{
    public class AccountFunctions : BaseFunction
    {
        public AccountFunctions(IEventSerializer eventSerializer, IMediator mediator) : base(eventSerializer, mediator)
        {
        }

        [FunctionName(nameof(AccountCreated))]
        public async Task AccountCreated([ServiceBusTrigger("aggregate-account", "created", Connection = "AzureWebJobsServiceBus")]ServiceBusMessage msg)
        {
            await HandleMessage(msg);
        }
    }
}