using MediatR;
using SuperSafeBank.Service.Core.Azure.Common.Persistence;
using SuperSafeBank.Service.Core.Common.Queries;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace SuperSafeBank.Service.Core.Azure.QueryHandlers
{
    public class CustomersArchiveHandler(IViewsContext dbContext) : IRequestHandler<CustomersArchive, IEnumerable<CustomerArchiveItem>>
    {
        private readonly IViewsContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task<IEnumerable<CustomerArchiveItem>> Handle(CustomersArchive request, CancellationToken cancellationToken)
        {
            var results = new List<CustomerArchiveItem>();

            var entities = _dbContext.CustomersArchive.QueryAsync<ViewTableEntity>(cancellationToken: cancellationToken);
            await foreach(var entity in entities)
            {
                var model = JsonSerializer.Deserialize<CustomerArchiveItem>(entity.Data);
                results.Add(model);
            }

            return results;
        }
    }
}