using System;
using System.Linq;
using FluentAssertions;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Domain.Events;
using Xunit;

namespace SuperSafeBank.Domain.Tests
{
    public class CustomerTests
    {
        [Fact]
        public void Create_should_create_valid_Customer_instance()
        {
            var customer = new Customer(Guid.NewGuid(), "lorem", "ipsum");

            var instance = BaseAggregateRoot<Customer, Guid>.Create(customer.Events);
            instance.Should().NotBeNull();
            instance.Id.Should().Be(customer.Id);
            instance.Firstname.Should().Be(customer.Firstname);
            instance.Lastname.Should().Be(customer.Lastname);
        }

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