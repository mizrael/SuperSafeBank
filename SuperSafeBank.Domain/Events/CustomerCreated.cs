using System;
using SuperSafeBank.Core.Models;

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

        public string Firstname { get; private set; }
        public string Lastname { get; private set; }
        public string Email { get; private set; }
    }
}