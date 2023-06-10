# SuperSafeBank 

[![SuperSafeBank](https://circleci.com/gh/mizrael/SuperSafeBank.svg?style=shield)](https://app.circleci.com/pipelines/github/mizrael/SuperSafeBank)

This repository shows how to implement Event Sourcing, CQRS and DDD in .NET Core, using a Bank as example.

The code has been used as example accompaining a few series of articles on [my personal blog](https://www.davidguida.net): 
- https://www.davidguida.net/event-sourcing-in-net-core-part-1-a-gentle-introduction/
- https://www.davidguida.net/event-sourcing-on-azure-part-1-architecture-plan/
- https://www.davidguida.net/event-sourcing-on-azure-part-2-events-persistence/
- https://www.davidguida.net/event-sourcing-on-azure-part-3-command-validation/
- https://www.davidguida.net/event-sourcing-on-azure-part-4-integration-events/
- https://www.davidguida.net/my-event-sourcing-journey-so-far/
- https://www.davidguida.net/event-sourcing-things-to-consider/

An ASP.NET Core API is used as entry-point for all the client-facing operations:
- create customers
- create accounts
- deposit money
- withdraw money

## Infrastructure
The Cloud can be hosted on Azure, using Azure Functions, [Storage Table](https://azure.microsoft.com/en-ca/services/storage/tables/?WT.mc_id=DOP-MVP-5003878) to persist Events and Materialized Views, and [ServiceBus](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview?WT.mc_id=DOP-MVP-5003878) to broadcast the Events.

An "on-premise" version is available as well, which uses
- [EventStore](https://eventstore.com/) or SQLServer can be used as persistence layer for aggregates.
- Kafka to broadcast the integration events
- MongoDb to store the QueryModels used by the API

The on-premise infrastructure can be spin up by simply running `docker-compose up` from the root folder. 

## Give a Star! ⭐️
Did you like this project? Give it a star, fork it, send me a PR or sponsor me!
