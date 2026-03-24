# Mapper Class Conversion Prompt

Convert the following original Mapper class to the new Mapper pattern using these rules:

## Conversion Rules

### 1. Base Class & Attributes
- **Old:** `MapperBase<TSource, TDest>` → **New:** `MapperProfile<TSource, TDest>`
- **Old:** `[RegisterSingleton<IMapper<TSource, TDest>>]` → **New:** `[RegisterSingleton]`
- **Add** `[GenerateMapper]` attribute to the class
- **Add** `partial` modifier to the class declaration

### 2. Method Signature
- **Old:** `protected override Expression<Func<TSource, TDest>> CreateMapping()` with object initializer
- **New:** `protected override void ConfigureMapping(MappingBuilder<TSource, TDest> mapping)` with builder syntax

### 3. Simple Property Assignments — REMOVE
If the mapping is a direct `dest.X = source.X` where names match exactly with no nested property path, **remove it entirely**. The new Mapper auto-maps these by convention.

Examples of simple mappings to remove:
```csharp
Title = source.Title,           // REMOVE — name matches, no nesting
Id = source.Id,                 // REMOVE — name matches, no nesting
RowVersion = source.RowVersion, // REMOVE — name matches, no nesting
```

### 4. Nested Property Navigation — Use Builder
If the mapping navigates through a child object (e.g., `source.Child.Property`), convert to builder syntax.

Old pattern (with null check):
```csharp
TenantName = source.Tenant != null ? source.Tenant.Name : null,
```

New pattern:
```csharp
mapping
    .Property(dest => dest.TenantName)
    .From(src => src.Tenant.Name);
```

### 5. Complex Expressions — Use Builder
If the mapping uses any expression beyond simple property assignment (calculations, string concatenation, conditional logic, method calls), convert to builder syntax.

Old pattern:
```csharp
FullName = source.FirstName + " " + source.LastName,
DisplayDate = source.Date.ToString("yyyy-MM-dd"),
```

New pattern:
```csharp
mapping
    .Property(dest => dest.FullName)
    .From(src => src.FirstName + " " + src.LastName);
mapping
    .Property(dest => dest.DisplayDate)
    .From(src => src.Date.ToString("yyyy-MM-dd"));
```

### 6. ConfigureMapping Override
- Only include `ConfigureMapping` if there are builder mappings to define
- If every property is a simple name-match, the override is not needed

---

## Template

```csharp
[GenerateMapper]
[RegisterSingleton]
internal sealed partial class {ClassName}
    : MapperProfile<{TSource}, {TDest}>
{
    // Only include if there are non-trivial mappings
    protected override void ConfigureMapping(MappingBuilder<{TSource}, {TDest}> mapping)
    {
        // Builder mappings here
    }
}
```

---

## Example Conversion

### Before
```csharp
[RegisterSingleton<IMapper<Entities.Task, Models.TaskReadModel>>]
internal sealed class TaskToTaskReadModelMapper
    : MapperBase<Entities.Task, Models.TaskReadModel>
{
    protected override Expression<Func<Entities.Task, Models.TaskReadModel>> CreateMapping()
    {
        return source => new Models.TaskReadModel
        {
            Title = source.Title,
            Description = source.Description,
            TenantId = source.TenantId,
            StatusId = source.StatusId,
            Id = source.Id,
            Created = source.Created,
            RowVersion = source.RowVersion,
            TenantName = source.Tenant != null ? source.Tenant.Name : null,
            StatusName = source.Status != null ? source.Status.Name : null,
            PriorityName = source.Priority != null ? source.Priority.Name : null,
        };
    }
}
```

### After
```csharp
[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TaskToTaskReadModelMapper
    : MapperProfile<Entities.Task, Models.TaskReadModel>
{
    protected override void ConfigureMapping(MappingBuilder<Entities.Task, Models.TaskReadModel> mapping)
    {
        mapping
            .Property(dest => dest.TenantName)
            .From(src => src.Tenant.Name);
        mapping
            .Property(dest => dest.StatusName)
            .From(src => src.Status.Name);
        mapping
            .Property(dest => dest.PriorityName)
            .From(src => src.Priority.Name);
    }
}
```
