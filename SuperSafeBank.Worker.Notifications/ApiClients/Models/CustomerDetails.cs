using System;
using Newtonsoft.Json;

namespace SuperSafeBank.Worker.Notifications.ApiClients.Models
{
    public class CustomerDetails
    {
        [JsonConstructor]
        private CustomerDetails(Guid id, string firstname, string lastname, string email)
        {
            Id = id;
            Firstname = firstname;
            Lastname = lastname;
            Email = email;
        }

        public Guid Id { get; private set; }
        public string Firstname { get; private set; }
        public string Lastname { get; private set; }
        public string Email { get; private set; }

    }
}