using System;

namespace SuperSafeBank.Domain.Queries.Models
{
    public class CustomerDetails
    {
        public CustomerDetails(Guid id, string firstname, string lastname, long version)
        {
            Id = id;
            Firstname = firstname;
            Lastname = lastname;
            Version = version;
        }

        public Guid Id { get; }
        public string Firstname { get; }
        public string Lastname { get; }
        public long Version { get; }
    }
}
