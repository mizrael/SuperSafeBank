using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Web.Core.Queries.Models
{
    public class CustomerDetails
    {
        private CustomerDetails()
        {
        }

        public CustomerDetails(Guid id, string firstname, string lastname, string email, IEnumerable<Guid> accounts, Money totalBalance)
        {
            Id = id;
            Firstname = firstname;
            Lastname = lastname;
            Email = email;
            Accounts = (accounts ?? Enumerable.Empty<Guid>()).ToArray();
            TotalBalance = totalBalance;
        }

        public Guid Id { get; private set; }
        public string Firstname { get; private set; }
        public string Lastname { get; private set; }
        public string Email { get; private set; }
        public Guid[] Accounts { get; private set; }
        public Money TotalBalance {get;private set; }
    }
}
