---
name: arbiter-endpoints
description: Use when exposing Arbiter commands and queries as REST via Minimal APIs — AddEndpointRoutes, MapEndpointRoutes, EntityCommandEndpointBase, EntityQueryEndpointBase, IEndpointRoute. Trigger on Minimal API endpoint registration for CRUD entities.
---

# Arbiter.CommandQuery.Endpoints

Minimal API surface that exposes the generic `Arbiter.CommandQuery` commands and queries as REST endpoints automatically. Each entity gets `GET /{prefix}/{name}/{id}`, `GET /{prefix}/{name}` (paged), `POST`, `PUT`, `PATCH`, `DELETE` with no per-route boilerplate.

## Install

```bash
dotnet add package Arbiter.CommandQuery.Endpoints
```

## Register

```csharp
using Arbiter.CommandQuery.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCommandQuery();
builder.Services.AddEndpointRoutes();

// One IEndpointRoute per entity (see below)
builder.Services.AddSingleton<IEndpointRoute, ProductEndpoint>();

var app = builder.Build();

// Mounts every IEndpointRoute under the prefix (default "/api")
app.MapEndpointRoutes();

app.Run();
```

`MapEndpointRoutes(prefix = "/api", serviceKey = null)` returns an `IEndpointConventionBuilder`, so chain `.RequireAuthorization()`, `.WithOpenApi()`, etc.

## Canonical pattern — one endpoint class per entity

```csharp
public class ProductEndpoint
    : EntityCommandEndpointBase<int, ProductReadModel, ProductReadModel, ProductCreateModel, ProductUpdateModel>
{
    public ProductEndpoint(ILoggerFactory loggerFactory)
        : base(loggerFactory, entityName: "Product") { }
}
```

That single class registers all of:

```
GET    /api/Product/{id}
GET    /api/Product
POST   /api/Product
PUT    /api/Product/{id}
PATCH  /api/Product/{id}
DELETE /api/Product/{id}
```

If you only need queries, derive from `EntityQueryEndpointBase<TKey, TListModel, TReadModel>` instead.

## Variations

```csharp
// Read-only entity (queries only)
public class CategoryEndpoint : EntityQueryEndpointBase<int, CategoryReadModel, CategoryReadModel>
{
    public CategoryEndpoint(ILoggerFactory l) : base(l, "Category") { }
}

// Custom routes — override MapEndpoints to add or replace any of the above
protected override void MapEndpoints(IEndpointRouteBuilder app)
{
    base.MapEndpoints(app);
    app.MapPost($"/{EntityName}/import", ImportProducts).WithName("ImportProducts");
}

// Auth applied via the convention builder returned by MapEndpointRoutes
app.MapEndpointRoutes().RequireAuthorization();
```

## Reference

- Minimal API guide: https://github.com/loresoft/Arbiter/blob/main/docs/guide/endpoints/minimal.md
- Sample: https://github.com/loresoft/Arbiter/tree/main/samples/EntityFramework
