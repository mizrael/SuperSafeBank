using System;

namespace SuperSafeBank.Domain
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