using System;
using MediatR;
using SuperSafeBank.Web.Core.Queries.Models;

namespace SuperSafeBank.Web.Core.Queries
{
    public class AccountById : IRequest<AccountDetails>
    {
        public AccountById(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}