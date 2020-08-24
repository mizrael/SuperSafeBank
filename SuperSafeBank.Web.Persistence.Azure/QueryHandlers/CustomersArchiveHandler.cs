using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SuperSafeBank.Web.Core.Queries;
using SuperSafeBank.Web.Core.Queries.Models;

namespace SuperSafeBank.Web.Persistence.Azure.QueryHandlers
{
    public class CustomersArchiveHandler : IRequestHandler<CustomersArchive, IEnumerable<CustomerArchiveItem>>
    {
        public async Task<IEnumerable<CustomerArchiveItem>> Handle(CustomersArchive request, CancellationToken cancellationToken)
        {
            return Enumerable.Empty<CustomerArchiveItem>();
        }
    }
}