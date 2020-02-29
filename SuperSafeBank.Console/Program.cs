using SuperSafeBank.Core.Models;
using System;

namespace SuperSafeBank.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var customer = Customer.Create("lorem", "ipsum");
            var account = Account.Create(customer, Currency.CanadianDollar);

            System.Console.WriteLine("Hello World!");
        }
    }
}
