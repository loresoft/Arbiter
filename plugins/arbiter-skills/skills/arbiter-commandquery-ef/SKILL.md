---
name: arbiter-commandquery-ef
description: Use when wiring Arbiter.CommandQuery with Entity Framework Core — registering AddEntityQueries / AddEntityCommands / AddQueryPipeline for an entity, defining DbContext-backed handlers, or implementing audited / soft-delete / tenant entities for EF Core.
---

# Arbiter.CommandQuery.EntityFramework

EF Core implementations of the generic `Arbiter.CommandQuery` handlers. Provides ready-made handlers for `EntityIdentifierQuery`, `EntityPagedQuery`, and the create/update/patch/delete commands against a `DbContext`.

## Install

```bash
dotnet add package Arbiter.CommandQuery.EntityFramework
```

Pairs with `Arbiter.CommandQuery` (base) and `Arbiter.Mapping` (read/update model mapping).

## Register

```csharp
using Arbiter.CommandQuery;
using Arbiter.CommandQuery.EntityFramework;

// EF Core DbContext
services.AddDbContext<TrackerContext>(o => o.UseSqlServer(connectionString));

// CQRS + mapping
services.AddCommandQuery();
services.AddSingleton<IMapper, ServiceProviderMapper>();
services.AddSingleton<IMapper<Product, ProductReadModel>,         ProductToReadModelMapper>();
services.AddSingleton<IMapper<ProductCreateModel, Product>,        ProductCreateModelToProductMapper>();
services.AddSingleton<IMapper<ProductUpdateModel, Product>,        ProductUpdateModelToProductMapper>();

// Per-entity registration
services.AddEntityQueries <TrackerContext, Product, int, ProductReadModel>();
services.AddEntityCommands<TrackerContext, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();
```

`AddEntityQueries` registers identifier, identifiers, and paged-query handlers.
`AddEntityCommands` registers create, update, patch, and delete handlers.

## Entity requirements

```csharp
public class Product : IHaveIdentifier<int>,
                       ITrackCreated, ITrackUpdated,   // audit fields
                       ITrackDeleted                   // soft delete
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }

    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset Updated { get; set; }
    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
}
```

Implementing `IHaveIdentifier<TKey>` is required. The other marker interfaces opt the entity into audit and soft-delete behaviors automatically — paged queries filter out soft-deleted rows; commands stamp audit fields from the `ClaimsPrincipal`.

## Finer-grained registration

Use these if you don't want every CRUD operation, or you want a custom handler for one:

```csharp
// Queries
services.AddEntityIdentifierQuery <int, ProductReadModel, MyHandler>();
services.AddEntityIdentifiersQuery<int, ProductReadModel, MyHandler>();
services.AddEntityPagedQuery      <ProductReadModel,      MyHandler>();

// Commands (DbContext-backed)
services.AddEntityCreateCommand<TrackerContext, Product, int, ProductReadModel, ProductCreateModel>();
services.AddEntityUpdateCommand<TrackerContext, Product, int, ProductReadModel, ProductUpdateModel>();
services.AddEntityPatchCommand <TrackerContext, Product, int, ProductReadModel>();
services.AddEntityDeleteCommand<TrackerContext, Product, int, ProductReadModel>();

// Commands (custom handler)
services.AddEntityCreateCommand<int, ProductReadModel, ProductCreateModel, MyCreateHandler>();
```

`AddQueryPipeline()` registers EF-specific pipeline pieces (call once; included automatically by the helpers above).

## Key-based lookups (for string/natural keys)

```csharp
// Entity implements IHaveKey + IHaveIdentifier<TKey>
services.AddEntityKeyQuery<TrackerContext, Product, ProductReadModel>();
```

## Reference

- EF handler guide: https://github.com/loresoft/Arbiter/blob/main/docs/guide/handlers/entityFramework.md
- Behaviors: https://github.com/loresoft/Arbiter/tree/main/docs/guide/behaviors
- Sample: https://github.com/loresoft/Arbiter/tree/main/samples/EntityFramework
