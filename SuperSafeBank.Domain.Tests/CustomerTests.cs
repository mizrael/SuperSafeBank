using System;
using System.Linq;
using FluentAssertions;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain.DomainEvents;
using Xunit;

namespace SuperSafeBank.Domain.Tests
{

    public class CustomerTests
    {
        [Fact]
        public void Create_from_events_should_create_valid_instance()
        {
            var customer = Customer.Create(Guid.NewGuid(), "lorem", "ipsum", "test@test.com");

            var sut = BaseAggregateRoot<Customer, Guid>.Create(customer.Events);
            sut.Should().NotBeNull();
            sut.Id.Should().Be(customer.Id);
            sut.Firstname.Should().Be(customer.Firstname);
            sut.Lastname.Should().Be(customer.Lastname);
            sut.Email.Should().NotBeNull();
            sut.Email.Should().Be(customer.Email);
            sut.Accounts.Should().BeEmpty();
        }

        [Fact]
        public void Create_should_create_valid_instance()
        {
            var sut = Customer.Create(Guid.NewGuid(), "lorem", "ipsum", "test@test.com");
            sut.Should().NotBeNull();
            sut.Id.Should().NotBeEmpty();
            sut.Firstname.Should().Be("lorem");
            sut.Lastname.Should().Be("ipsum");
            sut.Version.Should().Be(1);
            sut.Email.Should().NotBeNull();
            sut.Email.Value.Should().Be("test@test.com");
            sut.Accounts.Should().BeEmpty();
        }

        [Fact]
        public void ctor_should_raise_Created_event()
        {
            var expectedId = Guid.NewGuid();
            var sut = Customer.Create(expectedId, "lorem", "ipsum", "test@test.com");

            sut.Events.Count.Should().Be(1);

            var createdEvent = sut.Events.First() as CustomerEvents.CustomerCreated;
            createdEvent.Should().NotBeNull()
                .And.BeOfType<CustomerEvents.CustomerCreated>();
            createdEvent.AggregateId.Should().Be(expectedId);
            createdEvent.AggregateVersion.Should().Be(0);
            createdEvent.Firstname.Should().Be(sut.Firstname);
            createdEvent.Lastname.Should().Be(sut.Lastname);
            createdEvent.Email.Should().Be(sut.Email);
        }

        [Fact]
        public void AddAccount_should_not_add_account_twice()
        {
            var sut = Customer.Create(Guid.NewGuid(), "lorem", "ipsum", "test@test.com");

            var account = Account.Create(Guid.NewGuid(), sut, Currency.CanadianDollar);
            sut.Accounts.Should().Contain(account.Id);

            sut.AddAccount(account);
            sut.Accounts.Should().HaveCount(1)
                .And.Contain(account.Id);
        }
    }
}