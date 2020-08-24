using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SuperSafeBank.Web.Core.Queries;
using SuperSafeBank.Web.Core.Queries.Models;

namespace SuperSafeBank.Web.Persistence.Azure.QueryHandlers
{
    public class CustomerByIdHandler : IRequestHandler<CustomerById, CustomerDetails>
    {
        public async Task<CustomerDetails> Handle(CustomerById request, CancellationToken cancellationToken)
        {
            return null;
        }
    }
}