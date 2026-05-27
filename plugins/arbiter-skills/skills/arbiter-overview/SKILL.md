---
name: arbiter-overview
description: Use when a user mentions the Arbiter .NET library family or is choosing which Arbiter package to install. Routes to the specialist skill for the area in question — Mediation, CommandQuery, Mapping, Dispatcher, Communication, Services, OpenTelemetry, or Messaging.ServiceBus.
---

# Arbiter — package landscape

Arbiter (https://github.com/loresoft/Arbiter) is a family of small .NET libraries built around the Mediator pattern and CQRS. Pick packages à la carte.

## Package map

| Need | Package | Specialist skill |
| --- | --- | --- |
| In-process mediator (`IRequest`, `INotification`, pipeline behaviors) | `Arbiter.Mediation` | `arbiter-mediation` |
| CQRS commands/queries, filtering, paging, behaviors | `Arbiter.CommandQuery` | `arbiter-commandquery` |
| EF Core handlers for the base commands/queries | `Arbiter.CommandQuery.EntityFramework` | `arbiter-commandquery-ef` |
| MongoDB handlers for the base commands/queries | `Arbiter.CommandQuery.MongoDB` | `arbiter-commandquery-mongo` |
| Minimal API endpoints exposing commands/queries as REST | `Arbiter.CommandQuery.Endpoints` | `arbiter-endpoints` |
| MVC controllers exposing commands/queries | `Arbiter.CommandQuery.Mvc` | `arbiter-mvc` |
| Source-generated object mapping | `Arbiter.Mapping` (+ `.Generators`) | `arbiter-mapping` |
| Blazor dispatcher (WASM/Server/Auto), `ModelStateEditor` | `Arbiter.Dispatcher.Client` + `Arbiter.Dispatcher.Server` | `arbiter-dispatcher` |
| Email + SMS templates and delivery | `Arbiter.Communication` + `.Azure` / `.Graph` / `.Twilio` | `arbiter-communication` |
| CSV, encryption, caching, tokens, URL builder | `Arbiter.Services` | `arbiter-services` |
| OpenTelemetry tracing/metrics for Arbiter | `Arbiter.OpenTelemetry` + `.Server` / `.Monitor` | `arbiter-opentelemetry` |
| Azure Service Bus integration | `Arbiter.Messaging.ServiceBus` | `arbiter-messaging-servicebus` |

## Typical layered setup

A common Arbiter app stacks several packages:

```text
Arbiter.Mediation          ← always required
Arbiter.CommandQuery       ← if you want CQRS commands/queries
Arbiter.Mapping            ← for read-model / DTO mapping
Arbiter.CommandQuery.EntityFramework   ← pick one data provider
Arbiter.CommandQuery.Endpoints         ← pick one web surface
```

## Conventions to remember

- Handlers return `ValueTask<TResponse?>`.
- `EntityQuery` is the unified query shape (paged or non-paged); use `FilterOperators` / `SortDirections` enums (v2.0+).
- Mappers are source-generated via `[GenerateMapper]` + `MapperProfile<TSrc,TDst>`; register each closed mapper plus `ServiceProviderMapper`.
- Entity types implement `IHaveIdentifier<TKey>`. Tracked/audited/soft-delete/tenant behaviors are opt-in via marker interfaces.

## When in doubt

Ask Claude to load the specialist skill — e.g. *"use arbiter-commandquery-ef"* — or just describe the concrete task (*"register CRUD handlers for my Product entity with EF Core"*) and the matching skill will trigger automatically.

## Reference

- README: https://github.com/loresoft/Arbiter/blob/main/README.md
- Full docs: https://loresoft.github.io/Arbiter/
