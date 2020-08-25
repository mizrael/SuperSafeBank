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

        [FunctionName("Deposit")]
        public async Task Deposit([ServiceBusTrigger("aggregate-account", "deposit", Connection = "AzureWebJobsServiceBus")]Message msg)
        {
            await HandleMessage(msg);
        }

        [FunctionName("Withdraw")]
        public async Task Withdraw([ServiceBusTrigger("aggregate-account", "withdraw", Connection = "AzureWebJobsServiceBus")]Message msg)
        {
            await HandleMessage(msg);
        }

        [FunctionName("AccountDetails")]
        public async Task Details([ServiceBusTrigger("aggregate-account", "details", Connection = "AzureWebJobsServiceBus")]Message msg)
        {
            await HandleMessage(msg);
        }
    }
}