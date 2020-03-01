using System;

namespace SuperSafeBank.Core.Models
{
    public class Customer : BaseAggregateRoot<Customer, Guid>
    {
        public Customer(Guid id, string firstname, string lastname) : base(id)
        {
            Firstname = firstname;
            Lastname = lastname;
        }

        public string Firstname { get; }
        public string Lastname { get; }

        protected override void Apply(IDomainEvent<Guid> @event)
        {   
        }

        public static Customer Create(string firstName, string lastName)
        {
            return new Customer(Guid.NewGuid(), firstName, lastName);
        }
    }
}