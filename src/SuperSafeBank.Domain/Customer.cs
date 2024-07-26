using System;
using System.Collections.Generic;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain.DomainEvents;

namespace SuperSafeBank.Domain
{
    public record Customer : BaseAggregateRoot<Customer, Guid>
    {
        private readonly HashSet<Guid> _accounts = new();

        private Customer() { }
        
        public Customer(Guid id, string firstname, string lastname, Email email) : base(id)
        {
            if(string.IsNullOrWhiteSpace(firstname))
                throw new ArgumentNullException(nameof(firstname));
            if (string.IsNullOrWhiteSpace(lastname))
                throw new ArgumentNullException(nameof(lastname));
            if (email is null)            
                throw new ArgumentNullException(nameof(email));
            
            this.Append(new CustomerEvents.CustomerCreated(this, firstname, lastname, email));
        }

        public void AddAccount(Account account)
        {
            if (account is null)
                throw new ArgumentNullException(nameof(account));
            
            if (_accounts.Contains(account.Id))
                return;

            this.Append(new CustomerEvents.AccountAdded(this, account.Id));
        }

        public string Firstname { get; private set; }
        public string Lastname { get; private set; }
        public Email Email { get; private set; }
        public IReadOnlyCollection<Guid> Accounts => _accounts;

        protected override void When(IDomainEvent<Guid> @event)
        {
            switch (@event)
            {
                case CustomerEvents.CustomerCreated c:
                    this.Id = c.AggregateId;
                    this.Firstname = c.Firstname;
                    this.Lastname = c.Lastname;
                    this.Email = c.Email;
                    break;
                case CustomerEvents.AccountAdded aa:
                    _accounts.Add(aa.AccountId);
                    break;
            }
        }

        public static Customer Create(Guid customerId, string firstName, string lastName, string email) =>
            new Customer(customerId, firstName, lastName, new Email(email));
    }
}