using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using SuperSafeBank.Core;

namespace SuperSafeBank.Web.Persistence.Azure.Functions.Functions
{
    public class AccountFunctions : BaseFunction
    {
        public AccountFunctions(IEventSerializer eventSerializer, IMediator mediator) : base(eventSerializer, mediator)
        {
        }

        [FunctionName("AccountCreated")]
        public async Task AccountCreated([ServiceBusTrigger("aggregate-account", "created", Connection = "AzureWebJobsServiceBus")]Message msg)
        {
            await HandleMessage(msg);
        }
    }
}