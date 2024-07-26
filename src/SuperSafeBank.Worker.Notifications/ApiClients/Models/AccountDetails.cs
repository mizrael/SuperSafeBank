using System;

namespace SuperSafeBank.Worker.Notifications.ApiClients.Models
{
    public record AccountDetails(Guid Id, Guid OwnerId, string OwnerFirstName, string OwnerLastName, string OwnerEmail);
    
}