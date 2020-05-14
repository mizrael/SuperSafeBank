using System.Collections.Generic;
using MediatR;
using SuperSafeBank.Web.API.Queries.Models;

namespace SuperSafeBank.Web.API.Queries
{
    public class CustomersArchive : IRequest<IEnumerable<CustomerArchiveItem>> { }
}