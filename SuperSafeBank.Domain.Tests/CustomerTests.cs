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
            var customer = Customer.Create("lorem", "ipsum", "test@test.com");

            var instance = BaseAggregateRoot<Customer, Guid>.Create(customer.Events);
            instance.Should().NotBeNull();
            instance.Id.Should().Be(customer.Id);
            instance.Firstname.Should().Be(customer.Firstname);
            instance.Lastname.Should().Be(customer.Lastname);
            instance.Email.Should().NotBeNull();
            instance.Email.Should().Be(customer.Email);
        }

        [Fact]
        public void ctor_should_create_valid_instance()
        {
            var sut = Customer.Create("lorem", "ipsum", "test@test.com");
            
            sut.Id.Should().NotBeEmpty();
            sut.Firstname.Should().Be("lorem");
            sut.Lastname.Should().Be("ipsum");
            sut.Version.Should().Be(1);
            sut.Email.Should().NotBeNull();
            sut.Email.Value.Should().Be("test@test.com");
        }

        [Fact]
        public void ctor_should_raise_Created_event()
        {
            var sut = Customer.Create("lorem", "ipsum", "test@test.com");

            sut.Events.Count.Should().Be(1);

            var createdEvent = sut.Events.First() as CustomerCreated;
            createdEvent.Should().NotBeNull()
                .And.BeOfType<CustomerCreated>();
            createdEvent.AggregateId.Should().Be(sut.Id);
            createdEvent.AggregateVersion.Should().Be(0);
            createdEvent.Firstname.Should().Be(sut.Firstname);
            createdEvent.Lastname.Should().Be(sut.Lastname);
            createdEvent.Email.Should().Be(sut.Email);
        }
    }
}