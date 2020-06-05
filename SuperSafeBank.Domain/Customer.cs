using System;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Domain.Events;

namespace SuperSafeBank.Domain
{
    public class Customer : BaseAggregateRoot<Customer, Guid>
    {
        private Customer() { }
        
        public Customer(Guid id, string firstname, string lastname, string email) : base(id)
        {
            if(string.IsNullOrWhiteSpace(firstname))
                throw new ArgumentOutOfRangeException(nameof(firstname));
            if (string.IsNullOrWhiteSpace(lastname))
                throw new ArgumentOutOfRangeException(nameof(lastname));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentOutOfRangeException(nameof(email));

            Firstname = firstname;
            Lastname = lastname;
            Email = email;

            this.AddEvent(new CustomerCreated(this));
        }

        public string Firstname { get; private set; }
        public string Lastname { get; private set; }
        public string Email { get; private set; } //TODO: use value object instead of string

        protected override void Apply(IDomainEvent<Guid> @event)
        {
            switch (@event)
            {
                case CustomerCreated c:
                    this.Id = c.AggregateId;
                    this.Firstname = c.Firstname;
                    this.Lastname = c.Lastname;
                    break;
            }
        }

        public static Customer Create(string firstName, string lastName, string email)
        {
            return new Customer(Guid.NewGuid(), firstName, lastName, email);
        }
    }
}