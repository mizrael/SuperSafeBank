using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SuperSafeBank.Core;
using SuperSafeBank.Domain.Services;

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
        private readonly ICustomerEmailsService _customerEmailsRepository;


        public CreateCustomerHandler(IEventsService<Customer, Guid> eventsService, ICustomerEmailsService customerEmailsRepository)
        {
            _eventsService = eventsService ?? throw new ArgumentNullException(nameof(eventsService));
            _customerEmailsRepository = customerEmailsRepository ?? throw new ArgumentNullException(nameof(customerEmailsRepository));
        }

        public async Task Handle(CreateCustomer command, CancellationToken cancellationToken)
        {
            if (await _customerEmailsRepository.ExistsAsync(command.Email))
                throw new ValidationException("Unable to create Customer", new ValidationError(nameof(CreateCustomer.Email), $"email '{command.Email}' already exists"));

            var customer = new Customer(command.Id, command.FirstName, command.LastName, command.Email);
            await _eventsService.PersistAsync(customer);
            await _customerEmailsRepository.CreateAsync(command.Email, command.Id);
        }
    }
}