namespace SuperSafeBank.Web.API.Workers
{
    public static class EventReceivedFactory
    {
        public static EventReceived<TE> Create<TE>(TE @event)
        {
            return new EventReceived<TE>(@event);
        }
    }
}