# SuperSafeBank 

[![SuperSafeBank](https://circleci.com/gh/mizrael/SuperSafeBank.svg?style=shield)](https://app.circleci.com/pipelines/github/mizrael/SuperSafeBank)

This repository shows how to implement Event Sourcing, CQRS and DDD in .NET Core, using a Bank as example.

The code has been used as example accompaining a few series of articles on [my personal blog](https://www.davideguida.com): 
- https://www.davideguida.com/event-sourcing-in-net-core-part-1-a-gentle-introduction/
- https://www.davideguida.com/event-sourcing-on-azure-part-1-architecture-plan/

An ASP.NET Core API is used as entry-point for all the client-facing operations:
- create customers
- create accounts
- deposit money
- withdraw money

## Infrastructure
The Cloud can be hosted on Azure, using Azure Functions, [Storage Table](https://azure.microsoft.com/en-ca/services/storage/tables/?WT.mc_id=DOP-MVP-5003878) to persist Events and Materialized Views, and [ServiceBus](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview?WT.mc_id=DOP-MVP-5003878) to broadcast the Events.

An "on-premise" version is available as well, which uses
- [EventStore](https://eventstore.com/) to keep track of all the events
- Kafka to broadcast the integration events
- MongoDb to store the QueryModels used by the API

The on-premise infrastructure can be spin up by simply running `docker-compose up` from the root folder. 

## Give a Star! ⭐️
Did you like this project? Give it a star, fork it, send me a PR or sponsor me!
