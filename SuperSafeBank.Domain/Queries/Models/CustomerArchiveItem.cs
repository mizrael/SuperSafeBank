using System;

namespace SuperSafeBank.Domain.Queries.Models
{
    public class CustomerArchiveItem
    {
        public CustomerArchiveItem(Guid id, string firstname, string lastname, long version)
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