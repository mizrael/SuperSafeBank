using FluentAssertions;
using SuperSafeBank.Common.Models;
using System;
using Xunit;

namespace SuperSafeBank.Domain.Tests;

public class BaseAggregateRootTests
{
    [Fact]
    public void Create_from_events_should_empty_pending_events_collection()
    {
        var customer = Customer.Create(Guid.NewGuid(), "lorem", "ipsum", "test@test.com");

        var sut = BaseAggregateRoot<Customer, Guid>.Create(customer.NewEvents);
        sut.Should().NotBeNull();
        sut.NewEvents.Should().BeEmpty();
        sut.Version.Should().Be(1);
    }
}