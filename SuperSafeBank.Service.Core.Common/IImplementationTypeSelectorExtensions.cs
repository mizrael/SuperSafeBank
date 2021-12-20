using System;
using System.Collections.Generic;
using Scrutor;
using SuperSafeBank.Service.Core.Common.EventHandlers;

namespace SuperSafeBank.Service.Core.Common
{
    public static class IImplementationTypeSelectorExtensions
    {
        private static HashSet<Type> _decorators;

        static IImplementationTypeSelectorExtensions()
        {
            _decorators = new HashSet<Type>(new[]
            {
                typeof(RetryDecorator<>)
            });
        }

        public static IImplementationTypeSelector RegisterHandlers(this IImplementationTypeSelector selector, Type type)
        {
            return selector.AddClasses(c =>
                    c.AssignableTo(type)
                        .Where(t => !_decorators.Contains(t))
                )
                .UsingRegistrationStrategy(RegistrationStrategy.Append)
                .AsImplementedInterfaces()
                .WithScopedLifetime();
        }
    }
}