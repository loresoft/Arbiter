---
title: Arbiter API Reference
description: Arbiter API reference documentation for the Arbiter library ecosystem, including Mediator pattern implementation, CQRS architecture, and communication services for .NET applications.
---

# Arbiter Reference

The API Reference Documentation

## Overview

This API reference provides comprehensive documentation for the Arbiter library ecosystem, a collection of .NET libraries implementing the Mediator pattern and Command Query Responsibility Segregation (CQRS) architecture. The Arbiter suite enables clean, modular application design with support for vertical slice architecture.

## Library Components

### Core Libraries

- **Arbiter.Mediation** - Lightweight mediator pattern implementation with request/response handling, notifications, and pipeline behaviors
- **Arbiter.Mediation.OpenTelemetry** - OpenTelemetry tracing and metrics support for mediation operations

### Command Query Libraries

- **Arbiter.CommandQuery** - Foundation package providing base commands, queries, behaviors, and domain models
- **Arbiter.CommandQuery.EntityFramework** - Entity Framework Core handlers for CRUD operations and data access
- **Arbiter.CommandQuery.MongoDB** - MongoDB-specific handlers for document database operations
- **Arbiter.CommandQuery.Endpoints** - Minimal API endpoints for exposing commands and queries via HTTP
- **Arbiter.CommandQuery.Mvc** - ASP.NET Core MVC controllers for traditional web API development

### Communication Libraries

- **Arbiter.Communication** - Message templating and delivery abstraction for email and SMS services
- **Arbiter.Communication.Azure** - Integration with Azure Communication Services
- **Arbiter.Communication.Twilio** - Integration with SendGrid email and Twilio SMS services

## Key Features

### Mediation Pattern

- Request/response handling with `IRequest<TResponse>` and `IRequestHandler<TRequest, TResponse>`
- Event handling through `INotification` and `INotificationHandler<TNotification>`
- Pipeline behaviors for cross-cutting concerns like validation, caching, and logging

### CQRS Implementation

- Separation of command and query operations
- Entity-based CRUD commands (Create, Read, Update, Delete, Upsert, Patch)
- Advanced querying with filtering, sorting, paging, and continuation tokens
- Multi-tenant support with tenant isolation behaviors

### Data Access Patterns

- Repository pattern implementations for Entity Framework Core and MongoDB
- Automatic mapping between domain entities and view models
- Support for soft deletes, audit trails, and concurrency control
- Distributed and hybrid caching behaviors

### Web Integration

- Minimal API endpoints with automatic route generation
- MVC controllers with standardized CRUD operations
- Problem Details error handling

### Communication Services

- Template-based email and SMS messaging
- Support for multiple delivery providers (Azure, SendGrid, Twilio)
- Attachment handling for email services
- Configuration-driven service selection

## Navigation

Browse the API reference by namespace or use the search functionality to find specific types and members. Each documented type includes detailed information about its purpose, usage patterns, and code examples where applicable.
