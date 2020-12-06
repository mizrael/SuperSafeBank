using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperSafeBank.Web.Core.Queries.Models
{
    public class CustomerArchiveItem
    {
        private CustomerArchiveItem() { }

        public CustomerArchiveItem(Guid id, string firstname, string lastname, IEnumerable<Guid> accounts)
        {
            Id = id;
            Firstname = firstname;
            Lastname = lastname;
            Accounts = (accounts ?? Enumerable.Empty<Guid>()).ToArray();
        }

        [JsonProperty("id")]
        public Guid Id { get; private set; }
        public string Firstname { get; private set; }
        public string Lastname { get; private set; }       
        public Guid[] Accounts { get; private set; }

        
    }
}