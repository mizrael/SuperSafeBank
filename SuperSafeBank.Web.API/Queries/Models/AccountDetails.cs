using System;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Web.API.Queries.Models
{
    public class AccountDetails
    {
        private AccountDetails() { }

        public Guid Id { get; private set; }
        public Guid OwnerId { get; private set; }
        public string OwnerFirstName { get; private set; }
        public string OwnerLastName { get; private set; }
        public Money Balance { get; private set; }
        public long Version { get; private set; }
    }
}