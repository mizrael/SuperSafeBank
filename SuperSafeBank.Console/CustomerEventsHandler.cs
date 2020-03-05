using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SuperSafeBank.Domain.Events;

namespace SuperSafeBank.Console
{
    public class CustomerEventsHandler : INotificationHandler<EventReceived<CustomerCreated>>
    {
        public Task Handle(EventReceived<CustomerCreated> notification, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}