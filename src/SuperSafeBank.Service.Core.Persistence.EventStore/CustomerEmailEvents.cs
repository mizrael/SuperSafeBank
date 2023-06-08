using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Service.Core.Persistence.EventStore
{
    public static class CustomerEmailEvents
    {
        public record CustomerEmailCreated : BaseDomainEvent<CustomerEmail, string>
        {
            /// <summary>
            /// for deserialization
            /// </summary>
            private CustomerEmailCreated() { }

            public CustomerEmailCreated(CustomerEmail customer, string email, Guid customerId) : base(customer)
            {
                Email = email;
                CustomerId = customerId;
            }
            
            public string Email { get; init; }
            public Guid CustomerId { get; init; }
        }
    }
}