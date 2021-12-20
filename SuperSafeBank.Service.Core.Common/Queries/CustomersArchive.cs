using System;
using System.Collections.Generic;
using System.Linq;
using MediatR;

namespace SuperSafeBank.Service.Core.Common.Queries
{
    public record CustomerArchiveItem
    {
        public CustomerArchiveItem(Guid id, string firstname, string lastname, IEnumerable<Guid> accounts)
        {
            Id = id;
            Firstname = firstname;
            Lastname = lastname;
            Accounts = (accounts ?? Enumerable.Empty<Guid>()).ToArray();
        }

        public Guid Id { get; }
        public string Firstname { get; }
        public string Lastname { get; }
        public Guid[] Accounts { get; }
    }

    public class CustomersArchive : IRequest<IEnumerable<CustomerArchiveItem>> { }
}