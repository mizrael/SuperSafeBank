using System;
using MediatR;
using SuperSafeBank.Web.Core.Queries.Models;

namespace SuperSafeBank.Web.Core.Queries
{
    public class CustomerById : IRequest<CustomerDetails>
    {
        public CustomerById(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}