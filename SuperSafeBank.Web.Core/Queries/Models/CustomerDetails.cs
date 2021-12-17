using System;
using System.Collections.Generic;
using System.Linq;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Web.Core.Queries.Models
{
    public record CustomerDetails
    {
        public CustomerDetails(Guid id, string firstname, string lastname, string email, IEnumerable<Guid> accounts, Money totalBalance)
        {
            Id = id;
            Firstname = firstname;
            Lastname = lastname;
            Email = email;
            Accounts = (accounts ?? Enumerable.Empty<Guid>()).ToArray();
            TotalBalance = totalBalance;
        }

        public Guid Id { get; }
        public string Firstname { get; }
        public string Lastname { get; }
        public string Email { get; }
        public Guid[] Accounts { get; }
        public Money TotalBalance { get; }
    }
}
