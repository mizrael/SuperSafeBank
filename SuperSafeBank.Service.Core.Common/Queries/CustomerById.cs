using System;
using System.Collections.Generic;
using System.Linq;
using MediatR;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Service.Core.Common.Queries
{
    public record CustomerAccountDetails(Guid Id, Money Balance)
    {
        public static CustomerAccountDetails Map(Account account) 
            => new CustomerAccountDetails(account.Id, account.Balance);
    }

    public record CustomerDetails
    {
        public CustomerDetails(Guid id, string firstname, string lastname, string email, IEnumerable<CustomerAccountDetails> accounts, Money totalBalance)
        {
            Id = id;
            Firstname = firstname;
            Lastname = lastname;
            Email = email;
            Accounts = (accounts ?? Enumerable.Empty<CustomerAccountDetails>()).ToArray();
            TotalBalance = totalBalance;
        }

        public Guid Id { get; }
        public string Firstname { get; }
        public string Lastname { get; }
        public string Email { get; }
        public CustomerAccountDetails[] Accounts { get; }
        public Money TotalBalance { get; }
    }

    public record CustomerById(Guid CustomerId) : IRequest<CustomerDetails>;
}