using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SuperSafeBank.Core;

namespace SuperSafeBank.Domain.Commands
{
    public class CreateCustomer : INotification
    {
        public CreateCustomer(Guid id, string firstName, string lastName, string email)
        {
            this.Id = id;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Email = email;
        }

        public Guid Id { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Email { get; }
    }

    public class CreateCustomerHandler : INotificationHandler<CreateCustomer>
    {
        private readonly IEventsService<Customer, Guid> _eventsService;

        public CreateCustomerHandler(IEventsService<Customer, Guid> eventsService)
        {
            _eventsService = eventsService;
        }

        public async Task Handle(CreateCustomer command, CancellationToken cancellationToken)
        {
            //TODO email validation
            var customer = new Customer(command.Id, command.FirstName, command.LastName, command.Email);
            await _eventsService.PersistAsync(customer);
        }
    }
}