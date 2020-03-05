using System;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Console
{
    public class AccountView
    {
        public Guid Id;
        public Guid OwnerId;
        public Money Balance;
        public long Version;
    }
}