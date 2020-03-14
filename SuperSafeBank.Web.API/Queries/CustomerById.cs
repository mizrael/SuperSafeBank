using System;
using MediatR;
using SuperSafeBank.Domain.Queries.Models;

namespace SuperSafeBank.Web.API.Queries
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