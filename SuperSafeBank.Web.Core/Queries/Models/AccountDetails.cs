using System;
using Newtonsoft.Json;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Web.Core.Queries.Models
{
    public class AccountDetails
    {
        private AccountDetails() { }
        
        public AccountDetails(Guid id, Guid ownerId, string ownerFirstName, string ownerLastName, Money balance)
        {
            Id = id;
            OwnerId = ownerId;
            OwnerFirstName = ownerFirstName;
            OwnerLastName = ownerLastName;
            Balance = balance;
        }

        [JsonProperty("id")]
        public Guid Id { get; private set; }
        public Guid OwnerId { get; private set; }
        public string OwnerFirstName { get; private set; }
        public string OwnerLastName { get; private set; }
        public Money Balance { get; private set; }
    }
}