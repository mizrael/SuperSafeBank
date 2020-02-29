using System;

namespace SuperSafeBank.Core.Models
{
    public class AccountTransactionException : Exception
    {
        public Account Account { get; }

        public AccountTransactionException(string s, Account account) : base(s)
        {
            Account = account;
        }
    }
}