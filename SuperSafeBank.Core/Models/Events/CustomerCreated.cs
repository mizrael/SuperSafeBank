using System;

namespace SuperSafeBank.Core.Models.Events
{
    public class CustomerCreated : BaseDomainEvent<Customer, Guid>
    {
        /// <summary>
        /// for deserialization
        /// </summary>
        private CustomerCreated() { }

        public CustomerCreated(Customer customer) : base(customer)
        {
            Firstname = customer.Firstname;
            Lastname = customer.Lastname;
        }

        public string Firstname { get; private set; }
        public string Lastname { get; private set; }
    }
}