---
title: Entity Modifiers
description: Apply reusable save-side logic to entities inside the Entity Framework Core command handlers before changes are saved
---

# Entity Modifiers

Entity modifiers let you run reusable logic against an entity inside the Entity Framework Core command handlers, just before `SaveChangesAsync` is called. They are the save-side counterpart to query modifiers (`IQueryModifier<TEntity>`), which shape the queries used to read entities.

Use an entity modifier when a write needs work that must be committed in the same `SaveChanges` call as the entity. Examples:

- Adding related entities, such as making the current user the owner of a newly created record
- Setting derived or computed values
- Normalizing data before it is saved
- Recording an audit trail entry for the change

Without entity modifiers, this kind of logic usually requires replacing the generic command handler for each entity.

## Why Use Entity Modifiers

The command handlers save the entity and then read it back through the query pipeline to build the returned read model. If a query modifier restricts which rows a user can see, the newly saved entity has to be visible to that user when it is read back. Otherwise the command returns `null`.

For example, a query modifier might only return organizations the current user is a member of. A newly created organization has no members yet, so it is filtered out of the read back. An entity modifier can add the membership row before the save, so the organization is visible when it is read back:

```csharp
public class OrganizationOwnerModifier : IEntityModifier<Organization>
{
    public int Priority => 1;

    public ValueTask Apply(
        Organization entity,
        DbContext context,
        EntityChangeOperation operation,
        IRequestPrincipal? principal = null,
        CancellationToken cancellationToken = default)
    {
        if (operation != EntityChangeOperation.Created)
            return ValueTask.CompletedTask;

        var userId = principal?.Principal?.GetUserId();
        if (userId is null)
            return ValueTask.CompletedTask;

        var membership = new UserOrganization
        {
            UserId = userId.Value,
            Organization = entity,
            Role = "Owner",
            Created = principal.Activated,
            CreatedBy = principal.ActivatedBy,
        };

        entity.UserOrganizations.Add(membership);

        return ValueTask.CompletedTask;
    }
}
```

## Setup

Entity modifiers are applied by the query pipeline (`IQueryPipeline`), the same pipeline that applies query modifiers. Register the query pipeline, then register your modifiers as `IEntityModifier<TEntity>`:

```csharp
// register the query pipeline, which applies both query and entity modifiers
services.AddQueryPipeline();

// register entity modifiers
services.AddScoped<IEntityModifier<Organization>, OrganizationOwnerModifier>();
```

If `AddQueryPipeline()` is not called, the command handlers skip entity modifiers entirely.

Modifiers are resolved from the request's service scope. They can be registered with any lifetime and can depend on other services.

## When Modifiers Run

Every modifier receives the `EntityChangeOperation` being performed.

| Handler                      | Operation  | When the modifier runs                                                                   |
| ---------------------------- | ---------- | ---------------------------------------------------------------------------------------- |
| `EntityCreateCommandHandler` | `Created`  | After the entity is mapped, stamped with audit fields, and added to the `DbContext`       |
| `EntityUpdateCommandHandler` | `Updated`  | After the model is mapped onto the existing entity and the update audit fields are set |
| `EntityUpdateCommandHandler` | `Created`  | When upsert inserts a new entity, after it is added, mapped, and stamped                 |
| `EntityPatchCommandHandler`  | `Updated`  | After the JSON patch is applied and the update audit fields are set                    |
| `EntityDeleteCommandHandler` | `Deleted`  | Before the entity is removed, or before it is flagged as deleted when soft delete is used |

In every case the modifiers run before `SaveChangesAsync`. Any changes they make to the entity, and any entities they add through the `DbContext`, are saved in the same call.

## Modifier Parameters

`IEntityModifier<TEntity>.Apply` receives:

| Parameter   | Description                                                                                      |
| ----------- | ------------------------------------------------------------------------------------------------ |
| `entity`    | The entity being saved                                                                           |
| `context`   | The `DbContext` the handler uses. Changes made through it are saved with the entity              |
| `operation` | The `EntityChangeOperation` being performed: `Created`, `Updated` or `Deleted`                   |
| `principal` | The `IRequestPrincipal` that sent the command, if any. The command handlers pass the command itself |

`IRequestPrincipal` provides the `Principal` (`ClaimsPrincipal`), and the `Activated` timestamp and `ActivatedBy` user name that are also used for the audit fields.

## Ordering

Modifiers run one after another in ascending `Priority` order (lower values run first). Registration order does not matter.

```csharp
public class SlugModifier : IEntityModifier<Product>
{
    // run before modifiers that depend on the slug
    public int Priority => 0;

    public ValueTask Apply(
        Product entity,
        DbContext context,
        EntityChangeOperation operation,
        IRequestPrincipal? principal = null,
        CancellationToken cancellationToken = default)
    {
        if (operation is EntityChangeOperation.Created or EntityChangeOperation.Updated)
            entity.Slug = Slugify(entity.Name);

        return ValueTask.CompletedTask;
    }
}
```

## Named Pipelines

Named pipelines follow the same rules as the query pipeline. When a command is sent with a `FilterName`, only modifiers registered as keyed services under that name run. The default (non-keyed) modifiers do not run.

```csharp
// default pipeline
services.AddScoped<IEntityModifier<Organization>, OrganizationOwnerModifier>();

// also run the modifier for the "System" pipeline
services.AddKeyedScoped<IEntityModifier<Organization>, OrganizationOwnerModifier>("System");
```

```csharp
var command = new EntityCreateCommand<OrganizationCreateModel, OrganizationReadModel>(
    principal,
    createModel,
    filterName: "System");
```

The same `FilterName` selects the keyed query modifiers when the entity is read back.

## Customizing Handlers

The command handlers apply modifiers through the protected virtual `ApplyModifiers` method on `EntityDataContextHandlerBase`. Custom handlers that derive from it can call `ApplyModifiers` before saving. They can also override it to change how modifiers are applied. Handlers created without an `IQueryPipeline` skip the modifiers.

Code outside the handlers can apply entity modifiers directly through `IQueryPipeline`. The overloads mirror the query modifier overloads:

```csharp
// apply the default (non-keyed) entity modifiers
await pipeline.ApplyModifiers(entity, dataContext, EntityChangeOperation.Created, request, cancellationToken);

// apply the entity modifiers registered for a named pipeline
await pipeline.ApplyModifiers(entity, dataContext, EntityChangeOperation.Created, "System", request, cancellationToken);

await dataContext.SaveChangesAsync(cancellationToken);
```

## See Also

- [Entity Framework Core Handlers](entityFramework.md)
- [Create Command](../commands/create.md)
- [Update Command](../commands/update.md)
- [Patch Command](../commands/patch.md)
- [Delete Command](../commands/delete.md)
