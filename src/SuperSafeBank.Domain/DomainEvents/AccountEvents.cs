using System;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Domain.DomainEvents
{
    public static class AccountEvents
    {
        public record AccountCreated : BaseDomainEvent<Account, Guid>
        {
            /// <summary>
            /// for deserialization
            /// </summary>
            private AccountCreated() { }

            public AccountCreated(Account account, Customer owner, Currency currency) : base(account)
            {
                if (owner is null)
                    throw new ArgumentNullException(nameof(owner));

                OwnerId = owner.Id;
                Currency = currency ?? throw new ArgumentNullException(nameof(currency));
            }

            public Guid OwnerId { get; init; }
            public Currency Currency { get; init; }
        }

        public record Deposit : BaseDomainEvent<Account, Guid>
        {
            /// <summary>
            /// for deserialization
            /// </summary>
            private Deposit() { }

            public Deposit(Account account, Money amount) : base(account) => Amount = amount;

            public Money Amount { get; init; }
        }

        public record Withdrawal : BaseDomainEvent<Account, Guid>
        {
            /// <summary>
            /// for deserialization
            /// </summary>
            private Withdrawal() { }

            public Withdrawal(Account account, Money amount) : base(account) => Amount = amount;

            public Money Amount { get; init; }
        }
    }
}