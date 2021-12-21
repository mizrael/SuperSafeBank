using System;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Domain.Events
{
    public record CustomerCreated : BaseDomainEvent<Customer, Guid>
    {
        /// <summary>
        /// for deserialization
        /// </summary>
        private CustomerCreated() { }

        public CustomerCreated(Customer customer, string firstname, string lastname, Email email) : base(customer)
        {
            Firstname = firstname;
            Lastname = lastname;
            Email = email;
        }

        public string Firstname { get; init; }
        public string Lastname { get; init; }
        public Email Email { get; init; }
    }
}