using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using SuperSafeBank.Domain.Queries.Models;

namespace SuperSafeBank.Web.API.Queries
{
    public class CustomersArchiveHandler : IRequestHandler<CustomersArchive, IEnumerable<CustomerArchiveItem>>
    {
        private readonly IMongoDatabase _db;
        private readonly IMongoCollection<CustomerArchiveItem> _coll;

        public CustomersArchiveHandler(IMongoDatabase db)
        {
            _db = db;
            _coll = _db.GetCollection<CustomerArchiveItem>("customers");
        }

        public async Task<IEnumerable<CustomerArchiveItem>> Handle(CustomersArchive request, CancellationToken cancellationToken)
        {
            var filter = Builders<CustomerArchiveItem>.Filter.Empty;
            var cursor = await _coll.FindAsync(filter, null, cancellationToken);
            IEnumerable<CustomerArchiveItem> results = await cursor.ToListAsync(cancellationToken);
            return results;
        }
    }
}