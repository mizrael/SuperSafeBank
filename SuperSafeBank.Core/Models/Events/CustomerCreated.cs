using System;

namespace SuperSafeBank.Core.Models.Events
{
    public class CustomerCreated : BaseDomainEvent<Customer, Guid>
    {
        public CustomerCreated(Customer customer) : base(customer)
        {
            Firstname = customer.Firstname;
            Lastname = customer.Lastname;
        }

        public string Firstname { get; }
        public string Lastname { get; }
    }
}