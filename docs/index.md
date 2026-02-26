---
title: Arbiter Project
description: A comprehensive .NET library suite providing robust implementations of the Mediator pattern and CQRS patterns for clean, maintainable, and scalable architectures.
---
# Arbiter Project

Arbiter is a comprehensive .NET library suite that provides robust implementations of the **Mediator pattern** and **Command Query Responsibility Segregation (CQRS)** patterns. Designed for modern .NET applications, Arbiter enables clean, maintainable, and scalable architectures including Vertical Slice Architecture.

## Overview

Arbiter consists of multiple focused libraries that work together to provide a complete solution for building modular applications:

### Core Libraries

- **Mediation**: Lightweight mediator implementation for decoupled request/response handling
- **Command Query**: CQRS framework with pre-built commands, queries, and behaviors
- **Dispatcher**: Blazor-first `IDispatcher` abstraction for sending commands and queries from WASM and Server Interactive components
- **Services**: Utility library for CSV parsing, encryption, caching, token management, and URL building
- **Communication**: Message template system for email and SMS services

### Key Features

- **High Performance**: Optimized for minimal allocations and maximum throughput
- **Extensible**: Pipeline behaviors and customizable handlers
- **Architecture Support**: Perfect for Clean Architecture, Vertical Slice, and CQRS patterns
- **Observability**: Built-in OpenTelemetry support for tracing and metrics
- **Type Safe**: Strong typing throughout with comprehensive generic support
- **Multi-Database**: Support for Entity Framework Core, MongoDB, and more

## Quick Start

Get started with Arbiter in minutes:

```bash
dotnet add package Arbiter.Mediation
dotnet add package Arbiter.CommandQuery
```

## Documentation Structure

This documentation is organized into two main sections to help you get the most out of Arbiter:

### [User Guide](guide/quickStart.md)

The User Guide provides comprehensive tutorials, examples, and best practices:

- **[Quick Start](guide/quickStart.md)** - Get up and running in minutes
- **[Patterns](guide/patterns.md)** - Architectural patterns and design principles
- **[Mediation](guide/mediation.md)** - Core mediator functionality and usage
- **[Command Query](guide/commandQuery.md)** - CQRS implementation and patterns
- **[Mapping](guide/mapping/mapping.md)** - Data transformation strategies
- **[Queries](guide/queries/identifier.md)** - Query patterns, filtering, and caching
- **[Commands](guide/commands/create.md)** - Command operations for data manipulation
- **[Handlers](guide/handlers/entityFramework.md)** - Database-specific handler implementations
- **[Behaviors](guide/behaviors/delete.md)** - Cross-cutting concerns and pipeline behaviors
- **[Dispatcher](guide/dispatcher/overview.md)** - Blazor Dispatcher for WASM and Server Interactive modes
- **[Communication](guide/communication/overview.md)** - Message templates and delivery services

### [API Reference](reference/index.md)

The API Reference contains detailed documentation for all types, methods, and interfaces in the Arbiter libraries. Use this section when you need specific implementation details or are exploring the full capabilities of the framework.

## Package Library Matrix

| Library                                                                                                      | Package                                                                                                                                                                                  | Description                                                       |
| :----------------------------------------------------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------- |
| [Arbiter.Mediation](https://www.nuget.org/packages/Arbiter.Mediation/)                                       | [![Arbiter.Mediation](https://img.shields.io/nuget/v/Arbiter.Mediation.svg)](https://www.nuget.org/packages/Arbiter.Mediation/)                                                          | Lightweight and extensible implementation of the Mediator pattern |
| [Arbiter.Mediation.OpenTelemetry](https://www.nuget.org/packages/Arbiter.Mediation.OpenTelemetry/)           | [![Arbiter.Mediation.OpenTelemetry](https://img.shields.io/nuget/v/Arbiter.Mediation.OpenTelemetry.svg)](https://www.nuget.org/packages/Arbiter.Mediation.OpenTelemetry/)                | OpenTelemetry support for Arbiter.Mediation library               |
| [Arbiter.CommandQuery](https://www.nuget.org/packages/Arbiter.CommandQuery/)                                 | [![Arbiter.CommandQuery](https://img.shields.io/nuget/v/Arbiter.CommandQuery.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery/)                                                 | Base package for Commands, Queries and Behaviors                  |
| [Arbiter.CommandQuery.EntityFramework](https://www.nuget.org/packages/Arbiter.CommandQuery.EntityFramework/) | [![Arbiter.CommandQuery.EntityFramework](https://img.shields.io/nuget/v/Arbiter.CommandQuery.EntityFramework.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery.EntityFramework/) | Entity Framework Core handlers for the base Commands and Queries  |
| [Arbiter.CommandQuery.MongoDB](https://www.nuget.org/packages/Arbiter.CommandQuery.MongoDB/)                 | [![Arbiter.CommandQuery.MongoDB](https://img.shields.io/nuget/v/Arbiter.CommandQuery.MongoDB.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery.MongoDB/)                         | Mongo DB handlers for the base Commands and Queries               |
| [Arbiter.CommandQuery.Endpoints](https://www.nuget.org/packages/Arbiter.CommandQuery.Endpoints/)             | [![Arbiter.CommandQuery.Endpoints](https://img.shields.io/nuget/v/Arbiter.CommandQuery.Endpoints.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery.Endpoints/)                   | Minimal API endpoints for base Commands and Queries               |
| [Arbiter.CommandQuery.Mvc](https://www.nuget.org/packages/Arbiter.CommandQuery.Mvc/)                         | [![Arbiter.CommandQuery.Mvc](https://img.shields.io/nuget/v/Arbiter.CommandQuery.Mvc.svg)](https://www.nuget.org/packages/Arbiter.CommandQuery.Mvc/)                                     | MVC Controllers for base Commands and Queries                     |
| [Arbiter.Communication](https://www.nuget.org/packages/Arbiter.Communication/)                               | [![Arbiter.Communication](https://img.shields.io/nuget/v/Arbiter.Communication.svg)](https://www.nuget.org/packages/Arbiter.Communication/)                                              | Message template communication for email and SMS services         |
| [Arbiter.Communication.Azure](https://www.nuget.org/packages/Arbiter.Communication.Azure/)                   | [![Arbiter.Communication.Azure](https://img.shields.io/nuget/v/Arbiter.Communication.Azure.svg)](https://www.nuget.org/packages/Arbiter.Communication.Azure/)                            | Communication implementation for Azure Communication Services     |
| [Arbiter.Communication.Twilio](https://www.nuget.org/packages/Arbiter.Communication.Twilio/)                 | [![Arbiter.Communication.Twilio](https://img.shields.io/nuget/v/Arbiter.Communication.Twilio.svg)](https://www.nuget.org/packages/Arbiter.Communication.Twilio/)                         | Communication implementation for SendGrid and Twilio              |
| [Arbiter.Services](https://www.nuget.org/packages/Arbiter.Services/)                                         | [![Arbiter.Services](https://img.shields.io/nuget/v/Arbiter.Services.svg)](https://www.nuget.org/packages/Arbiter.Services/)                                                             | Utility services for CSV, encryption, caching, and tokens         |
| [Arbiter.Dispatcher.Server](https://www.nuget.org/packages/Arbiter.Dispatcher.Server/)                       | [![Arbiter.Dispatcher.Server](https://img.shields.io/nuget/v/Arbiter.Dispatcher.Server.svg)](https://www.nuget.org/packages/Arbiter.Dispatcher.Server/)                                  | Server-side endpoint for Blazor WASM dispatcher requests          |
| [Arbiter.Dispatcher.Client](https://www.nuget.org/packages/Arbiter.Dispatcher.Client/)                       | [![Arbiter.Dispatcher.Client](https://img.shields.io/nuget/v/Arbiter.Dispatcher.Client.svg)](https://www.nuget.org/packages/Arbiter.Dispatcher.Client/)                                  | Client dispatcher for WASM and Server Interactive modes           |

## Next Steps

Ready to get started? Check out the **[Quick Start Guide](guide/quickStart.md)** to build your first Arbiter application, or explore the **[User Guide](guide/quickStart.md)** for comprehensive tutorials and examples.

For detailed API information, browse the **[API Reference](reference/index.md)** section.
