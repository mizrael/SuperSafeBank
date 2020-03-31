using System;
using System.Collections.Generic;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Web.API.Queries.Models
{
    public class CustomerDetails
    {
        private CustomerDetails() { }
        public Guid Id { get; private set; }
        public string Firstname { get; private set; }
        public string Lastname { get; private set; }
        public IEnumerable<Guid> Accounts { get; private set; }
        public Money TotalBalance {get;private set; }
        public long Version { get; private set; }
    }
}
