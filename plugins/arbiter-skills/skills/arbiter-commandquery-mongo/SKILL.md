---
name: arbiter-commandquery-mongo
description: Use when wiring Arbiter.CommandQuery with MongoDB — AddMongoRepository plus AddEntityQueries / AddEntityCommands for an IMongoEntityRepository<TEntity>. Trigger on IMongoRepository, IMongoEntityRepository, MongoDB document handlers.
---

# Arbiter.CommandQuery.MongoDB

MongoDB implementations of the generic `Arbiter.CommandQuery` handlers. Uses `IMongoRepository<TEntity, TKey>` (typically resolved as `IMongoEntityRepository<TEntity>`) as the data gateway.

## Install

```bash
dotnet add package Arbiter.CommandQuery.MongoDB
```

Pairs with `Arbiter.CommandQuery` (base) and `Arbiter.Mapping`.

## Register

```csharp
using Arbiter.CommandQuery;
using Arbiter.CommandQuery.MongoDB;

// MongoDB repository services (connection-string name)
services.AddMongoRepository("Tracker");

// CQRS + mapping
services.AddCommandQuery();
services.AddSingleton<IMapper, ServiceProviderMapper>();
services.AddSingleton<IMapper<Product, ProductReadModel>,         ProductToReadModelMapper>();
services.AddSingleton<IMapper<ProductCreateModel, Product>,        ProductCreateModelToProductMapper>();
services.AddSingleton<IMapper<ProductUpdateModel, Product>,        ProductUpdateModelToProductMapper>();

// Per-entity registration (TRepository is the concrete repo interface)
services.AddEntityQueries <IMongoEntityRepository<Product>, Product, string, ProductReadModel>();
services.AddEntityCommands<IMongoEntityRepository<Product>, Product, string, ProductReadModel, ProductCreateModel, ProductUpdateModel>();
```

## Entity requirements

```csharp
public class Product : IHaveIdentifier<string>,
                       ITrackCreated, ITrackUpdated
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";

    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset Updated { get; set; }
    public string? UpdatedBy { get; set; }
}
```

Mongo entities typically use `string` keys. Marker interfaces (`ITrackCreated`, `ITrackUpdated`, `ITrackDeleted`, `IHaveTenant<TKey>`) opt into the standard behaviors just like the EF flavor — see `arbiter-commandquery`.

## Finer-grained registration

```csharp
services.AddEntityIdentifierQuery <string, ProductReadModel, MyHandler>();
services.AddEntityPagedQuery      <ProductReadModel,         MyHandler>();

services.AddEntityCreateCommand<IMongoEntityRepository<Product>, Product, string, ProductReadModel, ProductCreateModel>();
services.AddEntityUpdateCommand<IMongoEntityRepository<Product>, Product, string, ProductReadModel, ProductUpdateModel>();
services.AddEntityPatchCommand <IMongoEntityRepository<Product>, Product, string, ProductReadModel>();
services.AddEntityDeleteCommand<IMongoEntityRepository<Product>, Product, string, ProductReadModel>();
```

Key-based (natural-key) lookup:

```csharp
services.AddEntityKeyQuery<IMongoEntityRepository<Product>, Product, string, ProductReadModel>();
```

## Sending commands

Same as the EF flavor — the handlers conform to the shared `Arbiter.CommandQuery` request types, so callers don't change:

```csharp
var product = await mediator.Send(
    new EntityIdentifierQuery<string, ProductReadModel>(principal, id), ct);
```

## Reference

- Mongo handler guide: https://github.com/loresoft/Arbiter/blob/main/docs/guide/handlers/mongo.md
- Sample: https://github.com/loresoft/Arbiter/tree/main/samples/MongoDB
