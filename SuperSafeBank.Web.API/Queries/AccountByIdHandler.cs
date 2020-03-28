using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using SuperSafeBank.Domain.Queries.Models;
using SuperSafeBank.Web.API.Infrastructure;

namespace SuperSafeBank.Web.API.Queries
{
    public class AccountByIdHandler : IRequestHandler<AccountById, AccountDetails>
    {
        private readonly IQueryDbContext _db;

        public AccountByIdHandler(IQueryDbContext db)
        {
            _db = db;
        }

        public async Task<AccountDetails> Handle(AccountById request, CancellationToken cancellationToken)
        {
            var cursor = await _db.AccountsDetails.FindAsync(c => c.Id == request.Id,
                null, cancellationToken);
            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }
    }
}