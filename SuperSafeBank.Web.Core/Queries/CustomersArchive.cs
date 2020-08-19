using System.Collections.Generic;
using MediatR;
using SuperSafeBank.Web.Core.Queries.Models;

namespace SuperSafeBank.Web.Core.Queries
{
    public class CustomersArchive : IRequest<IEnumerable<CustomerArchiveItem>> { }
}