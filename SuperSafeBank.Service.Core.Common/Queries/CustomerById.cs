using MediatR;
using SuperSafeBank.Domain;
using System;
using System.Linq;

namespace SuperSafeBank.Service.Core.Common.Queries
{
    public record CustomerAccountDetails(Guid Id, Money Balance)
    {
        public static CustomerAccountDetails Map(Account account) 
            => new CustomerAccountDetails(account.Id, account.Balance);
    }

    public record CustomerDetails
    {
        public CustomerDetails(Guid id, string firstname, string lastname, string email, CustomerAccountDetails[] accounts, Money totalBalance)
        {
            Id = id;
            Firstname = firstname;
            Lastname = lastname;
            Email = email;
            Accounts = (accounts ?? Enumerable.Empty<CustomerAccountDetails>()).ToArray();
            TotalBalance = totalBalance;
        }

        public Guid Id { get; init; }
        public string Firstname { get; init; }
        public string Lastname { get; init; }
        public string Email { get; init; }
        public CustomerAccountDetails[] Accounts { get; init; }
        public Money TotalBalance { get; init; }
    }

    public record CustomerById(Guid CustomerId) : IRequest<CustomerDetails>;
}