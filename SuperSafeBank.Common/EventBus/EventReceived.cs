using MediatR;

namespace SuperSafeBank.Common.EventBus
{
    public record EventReceived<TE> : INotification
    {
        public EventReceived(TE @event)
        {
            Event = @event;
        }

        public TE Event { get; }
    }
}