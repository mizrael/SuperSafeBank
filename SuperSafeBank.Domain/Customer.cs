using System;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain.Events;

namespace SuperSafeBank.Domain
{
    public class Customer : BaseAggregateRoot<Customer, Guid>
    {
        private Customer() { }
        
        public Customer(Guid id, string firstname, string lastname, Email email) : base(id)
        {
            if(string.IsNullOrWhiteSpace(firstname))
                throw new ArgumentNullException(nameof(firstname));
            if (string.IsNullOrWhiteSpace(lastname))
                throw new ArgumentNullException(nameof(lastname));
            
            Firstname = firstname;
            Lastname = lastname;
            Email = email ?? throw new ArgumentNullException(nameof(email));

            this.Append(new CustomerCreated(this));
        }

        public string Firstname { get; private set; }
        public string Lastname { get; private set; }
        public Email Email { get; private set; }

        protected override void When(IDomainEvent<Guid> @event)
        {
            switch (@event)
            {
                case CustomerCreated c:
                    this.Id = c.AggregateId;
                    this.Firstname = c.Firstname;
                    this.Lastname = c.Lastname;
                    this.Email = c.Email;
                    break;
            }
        }

        public static Customer Create(string firstName, string lastName, string email)
        {
            return new Customer(Guid.NewGuid(), firstName, lastName, new Email(email));
        }
    }
}