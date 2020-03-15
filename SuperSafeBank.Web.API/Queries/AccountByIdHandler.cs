using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using SuperSafeBank.Domain.Queries.Models;

namespace SuperSafeBank.Web.API.Queries
{
    public class AccountByIdHandler : IRequestHandler<AccountById, AccountDetails>
    {
        private readonly IMongoDatabase _db;
        private readonly IMongoCollection<AccountDetails> _coll;

        public AccountByIdHandler(IMongoDatabase db)
        {
            _db = db;
            _coll = _db.GetCollection<AccountDetails>("accounts");
        }

        public async Task<AccountDetails> Handle(AccountById request, CancellationToken cancellationToken)
        {
            var cursor = await _coll.FindAsync(c => c.Id == request.Id,
                null, cancellationToken);
            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }
    }
}