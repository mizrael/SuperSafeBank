using System;
using System.Collections.Generic;

namespace SuperSafeBank.Domain;

public record Currency
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

    public override string ToString()
        => Name;

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
        var normalizedCode = code.Trim().ToUpper();
        if(!Currencies.ContainsKey(normalizedCode))
            throw new ArgumentException($"Invalid code: '{code}'", nameof(code));
        return Currencies[normalizedCode];
    }
    
    public static Currency Euro => new Currency("EUR", "€");
    public static Currency CanadianDollar => new Currency("CAD", "CA$");
    public static Currency USDollar => new Currency("USD", "US$");

    #endregion Factory
}