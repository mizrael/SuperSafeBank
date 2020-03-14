using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Queries.Models;

namespace SuperSafeBank.Web.API.Queries
{
    public class CustomerByIdHandler : IRequestHandler<CustomerById, CustomerDetails>
    {
        private readonly IMongoDatabase _db;
        private readonly IMongoCollection<CustomerDetails> _coll;

        public CustomerByIdHandler(IMongoDatabase db)
        {
            _db = db;
            _coll = _db.GetCollection<CustomerDetails>("customerdetails");
        }

        public async Task<CustomerDetails> Handle(CustomerById request, CancellationToken cancellationToken)
        {
            var cursor = await _coll.FindAsync(c => c.Id == request.Id,
                null, cancellationToken);
            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }
    }
}