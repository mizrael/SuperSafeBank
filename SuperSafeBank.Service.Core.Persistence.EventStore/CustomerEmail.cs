using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Service.Core.Persistence.EventStore
{
    public record CustomerEmail : BaseAggregateRoot<CustomerEmail, string>
    {
        private CustomerEmail() { }

        public CustomerEmail(string email, Guid customerId) : base(email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"'{nameof(email)}' cannot be null or whitespace.", nameof(email));
            }

            base.Append(new CustomerEmailEvents.CustomerEmailCreated(this, email, customerId));
        }

        public string Email { get; private set; }
        public Guid CustomerId { get; private set;  }

        protected override void When(IDomainEvent<string> @event)
        {
            switch (@event)
            {
                case CustomerEmailEvents.CustomerEmailCreated c:
                    this.Id = c.AggregateId;
                    this.Email = c.Email;
                    this.CustomerId = c.CustomerId;
                    break;
            }
        }
    }   
}