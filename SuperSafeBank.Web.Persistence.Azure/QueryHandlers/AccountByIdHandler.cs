using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SuperSafeBank.Web.Core.Queries;
using SuperSafeBank.Web.Core.Queries.Models;

namespace SuperSafeBank.Web.Persistence.Azure.QueryHandlers
{
    public class AccountByIdHandler : IRequestHandler<AccountById, AccountDetails>
    {
        public async Task<AccountDetails> Handle(AccountById request, CancellationToken cancellationToken)
        {
            return null;
        }
    }
}