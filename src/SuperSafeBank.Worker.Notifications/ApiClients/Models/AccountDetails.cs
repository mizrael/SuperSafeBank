using System;

namespace SuperSafeBank.Worker.Notifications.ApiClients.Models
{
    public record AccountDetails
    {
        public Guid Id { get; init; }
        public Guid OwnerId { get; init; }
        public string OwnerFirstName { get; init; }
        public string OwnerLastName { get; init; }
        public string OwnerEmail { get; init; }
    }
}