---
name: arbiter-commandquery
description: Use when working with Arbiter's CQRS base layer — EntityQuery, EntityFilter, FilterOperators, SortDirections, EntityPagedQuery, EntityIdentifierQuery, EntityCreateCommand / EntityUpdateCommand / EntityPatchCommand / EntityDeleteCommand, or pipeline behaviors like validation, hybrid cache, audit, tenant, soft-delete. Trigger on AddCommandQuery, AddCommandValidation, AddEntityHybridCache.
---

# Arbiter.CommandQuery

CQRS layer on top of `Arbiter.Mediation`: pre-built generic commands/queries for entity CRUD, plus filter/sort/page modeling and opt-in behaviors.

## Install

```bash
dotnet add package Arbiter.CommandQuery
```

A data provider package supplies the handlers — pick one:
- `Arbiter.CommandQuery.EntityFramework` → see `arbiter-commandquery-ef`
- `Arbiter.CommandQuery.MongoDB` → see `arbiter-commandquery-mongo`

## Register

```csharp
using Arbiter.CommandQuery;

services.AddCommandQuery();              // core CQRS pipeline + mediator
services.AddCommandValidation();         // optional: FluentValidation pipeline behavior
services.AddEntityHybridCache();         // optional: HybridCache-backed query caching
services.AddMessagePackOptions();        // optional: configure MessagePack for dispatcher
```

## Built-in commands and queries

| Type | Purpose |
| --- | --- |
| `EntityIdentifierQuery<TKey, TReadModel>` | Get one by id |
| `EntityIdentifiersQuery<TKey, TReadModel>` | Get many by ids |
| `EntityPagedQuery<TReadModel>` | Filter + sort + (optional) page |
| `EntityCreateCommand<TCreateModel, TReadModel>` | Insert |
| `EntityUpdateCommand<TKey, TUpdateModel, TReadModel>` | Update; pass `upsert: true` for upsert |
| `EntityPatchCommand<TKey, TReadModel>` | JSON Patch partial update |
| `EntityDeleteCommand<TKey, TReadModel>` | Delete |

All commands/queries take a `ClaimsPrincipal` as the first ctor arg.

## Canonical pattern — paged query with filter + sort

```csharp
using Arbiter.CommandQuery.Queries;

var entityQuery = new EntityQuery
{
    Filter = new EntityFilter
    {
        Logic = FilterLogic.And,
        Filters = new List<EntityFilter>
        {
            new() { Name = "Category", Operator = FilterOperators.Equal,       Value = "Electronics" },
            new() { Name = "Price",    Operator = FilterOperators.GreaterThan, Value = 100m },
        }
    },
    Sort = new List<EntitySort>
    {
        new() { Name = "Name", Direction = SortDirections.Ascending }
    },
    Page = 1,
    PageSize = 20,   // omit Page/PageSize to return all matches
};

var query  = new EntityPagedQuery<ProductReadModel>(principal, entityQuery);
var result = await mediator.Send(query, cancellationToken); // EntityPagedResult<ProductReadModel>
```

## Variations

```csharp
// Get by id
var product = await mediator.Send(
    new EntityIdentifierQuery<int, ProductReadModel>(principal, id), ct);

// Create
var created = await mediator.Send(
    new EntityCreateCommand<ProductCreateModel, ProductReadModel>(principal, createModel), ct);

// Update (upsert with last bool)
var updated = await mediator.Send(
    new EntityUpdateCommand<int, ProductUpdateModel, ProductReadModel>(principal, id, model, upsert: true), ct);

// Delete
var deleted = await mediator.Send(
    new EntityDeleteCommand<int, ProductReadModel>(principal, id), ct);
```

## Filter operators (enum, v2.0+)

`FilterOperators`: `Equal`, `NotEqual`, `LessThan`, `LessThanOrEqual`, `GreaterThan`, `GreaterThanOrEqual`, `Contains`, `StartsWith`, `EndsWith`, `In`, `NotIn`, `IsNull`, `IsNotNull`.
`FilterLogic`: `And`, `Or`.
`SortDirections`: `Ascending`, `Descending`.

## Behaviors (opt-in via marker interfaces on the entity / model)

| Behavior | Triggered by |
| --- | --- |
| Validation | `services.AddCommandValidation()` + a `FluentValidation.IValidator<TRequest>` |
| Hybrid cache | `services.AddEntityHybridCache()` + `[Cacheable]` on the query/read-model |
| Audit fields | Entity implements `ITrackCreated` / `ITrackUpdated` |
| Soft delete | Entity implements `ITrackDeleted` — paged queries auto-filter |
| Tenant isolation | Entity implements `IHaveTenant<TKey>` |

Add per-entity behavior registrations only if you need extra pipeline stages beyond what `AddEntityQueries` / `AddEntityCommands` already wires:

```csharp
services.AddEntityQueryBehaviors<int, ProductReadModel>();
services.AddEntityCreateBehaviors<int, ProductReadModel, ProductCreateModel>();
services.AddEntityUpdateBehaviors<int, ProductReadModel, ProductUpdateModel>();
services.AddEntityPatchBehaviors<int, Product, ProductReadModel>();
services.AddEntityDeleteBehaviors<int, Product, ProductReadModel>();
```

## Reference

- Guide: https://github.com/loresoft/Arbiter/blob/main/docs/guide/commandQuery.md
- Queries: https://github.com/loresoft/Arbiter/tree/main/docs/guide/queries
- Commands: https://github.com/loresoft/Arbiter/tree/main/docs/guide/commands
- Behaviors: https://github.com/loresoft/Arbiter/tree/main/docs/guide/behaviors
