using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using SuperSafeBank.Web.Core.Queries;
using SuperSafeBank.Web.Core.Queries.Models;

namespace SuperSafeBank.Web.Persistence.Mongo.QueryHandlers
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