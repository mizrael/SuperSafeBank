using System;

namespace SuperSafeBank.Worker.Notifications.ApiClients.Models
{
    public record CustomerDetails(Guid Id, string Firstname, string Lastname, string Email);
}