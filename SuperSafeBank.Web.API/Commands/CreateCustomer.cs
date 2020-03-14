using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SuperSafeBank.Core;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Web.API.Commands
{
    public class CreateCustomer : INotification
    {
        public CreateCustomer(Guid id, string firstName, string lastName)
        {
            this.Id = id;
            this.FirstName = firstName;
            this.LastName = lastName;
        }

        public Guid Id { get; }
        public string FirstName { get; }
        public string LastName { get; }
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
            var customer = new Customer(command.Id, command.FirstName, command.LastName);
            await _eventsService.PersistAsync(customer);
        }
    }
}