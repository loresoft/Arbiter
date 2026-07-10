---
name: arbiter-mvc
description: Use when exposing Arbiter commands and queries via MVC controllers — deriving from EntityCommandControllerBase or EntityQueryControllerBase rather than Minimal API endpoints. Trigger on MVC + CQRS controllers for an entity.
---

# Arbiter.CommandQuery.Mvc

ASP.NET Core MVC controller base classes that wrap the generic `Arbiter.CommandQuery` operations. Pick this over `arbiter-endpoints` if your app uses controllers, filters, model binding, or content negotiation features specific to MVC.

## Install

```bash
dotnet add package Arbiter.CommandQuery.Mvc
```

## Register

```csharp
using Arbiter.CommandQuery;

builder.Services.AddCommandQuery();
builder.Services.AddControllers();   // standard MVC

var app = builder.Build();
app.MapControllers();
```

There's no special `AddXxx` for the MVC package — controllers register through `AddControllers()` like any other.

## Canonical pattern — one controller per entity

```csharp
[Route("api/[controller]")]
public class ProductController
    : EntityCommandControllerBase<int, ProductReadModel, ProductReadModel, ProductCreateModel, ProductUpdateModel>
{
    public ProductController(IMediator mediator) : base(mediator) { }
}
```

That single class exposes:

```
GET    /api/Product/{id}
GET    /api/Product       (paged, with query string filter/sort)
POST   /api/Product
PUT    /api/Product/{id}
PATCH  /api/Product/{id}
DELETE /api/Product/{id}
```

## Variations

```csharp
// Read-only
[Route("api/[controller]")]
public class CategoryController : EntityQueryControllerBase<int, CategoryReadModel, CategoryReadModel>
{
    public CategoryController(IMediator mediator) : base(mediator) { }
}

// Add custom action alongside the inherited CRUD
[HttpPost("import")]
public async Task<IActionResult> Import([FromBody] ImportRequest request, CancellationToken ct)
{
    var result = await Mediator.Send(new ImportProducts(User, request), ct);
    return Ok(result);
}
```

Standard MVC features (`[Authorize]`, action filters, model binders) compose normally because each method on the base is a regular controller action.

## When to choose MVC over Endpoints

- Existing MVC-centric app with shared filters/conventions.
- Need full content negotiation (XML, etc.).
- Otherwise prefer `arbiter-endpoints` for lower overhead and minimal-API ergonomics.

## Reference

- Controller guide: https://github.com/loresoft/Arbiter/blob/main/docs/guide/endpoints/controller.md
