using System;

namespace SuperSafeBank.Web.Core.Queries.Models
{
    public class CustomerArchiveItem
    {
        private CustomerArchiveItem() { }
        public Guid Id { get; private set; }
        public string Firstname { get; private set; }
        public string Lastname { get; private set; }
        public Guid[] Accounts { get; private set; }
        public long Version { get; private set; }
    }
}