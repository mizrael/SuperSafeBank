# SuperSafeBank 

This repository shows how to implement Event Sourcing, CQRS and DDD in .NET Core, using a Bank as example.

An ASP.NET Core API is used as entry-point for all the client-facing operations:
- create customers
- create accounts
- deposit money
- withdraw money

The infrastructure can be spin up by simply running `docker-compose up` from the root folder. 

The system makes use of
- EventStore to keep track of all the events
- Kafka to broadcast the integration events
- MongoDb to store the QueryModels used by the API

The code has been used as example accompaining a series of articles on my personal blog: https://www.davideguida.com/event-sourcing-in-net-core-part-1-a-gentle-introduction/