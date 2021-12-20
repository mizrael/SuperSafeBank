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

        public CustomerCreated(Customer customer) : base(customer)
        {
            Firstname = customer.Firstname;
            Lastname = customer.Lastname;
            Email = customer.Email;
        }

        public string Firstname { get; init; }
        public string Lastname { get; init; }
        public Email Email { get; init; }
    }
}