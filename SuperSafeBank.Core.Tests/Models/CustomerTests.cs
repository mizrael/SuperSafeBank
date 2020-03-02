using System;
using System.Linq;
using FluentAssertions;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Core.Models.Events;
using Xunit;

namespace SuperSafeBank.Core.Tests.Models
{
    public class CustomerTests
    {
        [Fact]
        public void ctor_should_create_valid_instance()
        {
            var sut = Customer.Create("lorem", "ipsum");
            
            sut.Firstname.Should().Be("lorem");
            sut.Lastname.Should().Be("ipsum");
            sut.Version.Should().Be(1);
        }

        [Fact]
        public void ctor_should_raise_Created_event()
        {
            var customerId = Guid.NewGuid();
            var sut = new Customer(customerId, "lorem", "ipsum");

            sut.Events.Count.Should().Be(1);

            var createdEvent = sut.Events.First() as CustomerCreated;
            createdEvent.Should().NotBeNull()
                .And.BeOfType<CustomerCreated>();
            createdEvent.AggregateId.Should().Be(customerId);
            createdEvent.AggregateVersion.Should().Be(0);
            createdEvent.Firstname.Should().Be("lorem");
            createdEvent.Lastname.Should().Be("ipsum");
        }
    }
}