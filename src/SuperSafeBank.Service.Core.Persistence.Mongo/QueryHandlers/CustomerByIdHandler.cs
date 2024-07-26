using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using SuperSafeBank.Service.Core.Common.Queries;

namespace SuperSafeBank.Service.Core.Persistence.Mongo.QueryHandlers
{
    public class CustomerByIdHandler(IQueryDbContext db) : IRequestHandler<CustomerById, CustomerDetails>
    {
        private readonly IQueryDbContext _db = db;

        public async Task<CustomerDetails> Handle(CustomerById request, CancellationToken cancellationToken)
        {
            var cursor = await _db.CustomersDetails.FindAsync(c => c.Id == request.CustomerId,
                null, cancellationToken);
            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }
    }
}