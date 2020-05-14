using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using SuperSafeBank.Web.API.Infrastructure;
using SuperSafeBank.Web.API.Queries.Models;

namespace SuperSafeBank.Web.API.Queries
{
    public class CustomersArchiveHandler : IRequestHandler<CustomersArchive, IEnumerable<CustomerArchiveItem>>
    {
        private readonly IQueryDbContext _db;

        public CustomersArchiveHandler(IQueryDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<CustomerArchiveItem>> Handle(CustomersArchive request, CancellationToken cancellationToken)
        {
            var filter = Builders<CustomerArchiveItem>.Filter.Empty;
            var cursor = await _db.Customers.FindAsync(filter, null, cancellationToken);
            IEnumerable<CustomerArchiveItem> results = await cursor.ToListAsync(cancellationToken);
            return results;
        }
    }
}