using System;
using System.Collections.Generic;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Domain
{
    public class Currency : ValueObject<Currency>
    {
        public Currency(string name, string symbol)
        {
            if(string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentNullException(nameof(symbol));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            Symbol = symbol;
            Name = name;
        }

        public string Name { get; }
        public string Symbol { get; }

        protected override bool EqualsCore(Currency other)
        {
            return this.Symbol == other.Symbol;
        }

        protected override int GetHashCodeCore() => this.Symbol.GetHashCode();

        public override string ToString()
        {
            return this.Symbol;
        }

        #region Factory

        private static readonly IDictionary<string, Currency> Currencies;

        static Currency()
        {
            Currencies = new Dictionary<string, Currency>()
            {
                { Euro.Name, Euro },
                { CanadianDollar.Name, CanadianDollar },
                { USDollar.Name, USDollar },
            };
        }

        public static Currency FromCode(string code)
        {
            if(string.IsNullOrWhiteSpace(code))
                throw new ArgumentNullException(nameof(code));
            if(!Currencies.ContainsKey(code))
                throw new ArgumentException($"Invalid code: {code}", nameof(code));
            return Currencies[code];
        }
        
        public static Currency Euro => new Currency("EUR", "€");
        public static Currency CanadianDollar => new Currency("CAD", "CA$");
        public static Currency USDollar => new Currency("USD", "US$");

        #endregion Factory
    }
}