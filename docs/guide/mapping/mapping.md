---
title: Mapping Process
description: Source-generated, compile-time object mapping with support for custom property expressions and IQueryable projection
---

# Mapping Process

The Arbiter framework provides a source-generated, compile-time mapping system for transforming objects from one type to another. The mapping system uses a Roslyn incremental source generator to emit strongly-typed `Map` and `ProjectTo` implementations at build time — eliminating runtime reflection and providing excellent performance with full AOT compatibility.

## Installation

The `Arbiter.Mapping` package includes the source generator automatically:

```bash
dotnet add package Arbiter.Mapping
```

## Core Mapping Interfaces

### IMapper

The `IMapper` interface provides a type-agnostic contract for object mapping operations:

```csharp
public interface IMapper
{
    TDestination? Map<TSource, TDestination>(TSource? source);
    void Map<TSource, TDestination>(TSource source, TDestination destination);
    IQueryable<TDestination> ProjectTo<TSource, TDestination>(IQueryable<TSource> source);
}
```

This interface supports three main mapping scenarios:

1. **Creating new objects**: `Map<TSource, TDestination>()` creates a new destination instance
2. **Updating existing objects**: `Map<TSource, TDestination>(source, destination)` updates an existing destination
3. **Query projection**: `ProjectTo<TSource, TDestination>()` projects queryables for deferred execution

### IMapper&lt;TSource, TDestination&gt;

The generic `IMapper<TSource, TDestination>` interface provides strongly-typed mapping between specific source and destination types:

```csharp
public interface IMapper<in TSource, TDestination>
{
    TDestination? Map(TSource? source);
    void Map(TSource source, TDestination destination);
    IQueryable<TDestination> ProjectTo(IQueryable<TSource> source);
}
```

This interface is implemented by the source-generated mapper classes and registered in the dependency injection container for type-safe resolution.

## Interface Comparison: When to Use Which

### IMapper vs IMapper&lt;TSource, TDestination&gt;

Understanding the differences between these interfaces is crucial for choosing the right approach:

| Aspect            | IMapper                              | IMapper&lt;TSource, TDestination&gt;       |
| ----------------- | ------------------------------------ | ------------------------------------------ |
| **Type Safety**   | Runtime type specification           | Compile-time type safety                   |
| **Performance**   | Requires service resolution overhead | Direct method calls, better performance    |
| **Flexibility**   | Works with any type combination      | Fixed to specific source/destination types |
| **Registration**  | Single registration for all mappings | Individual registration per type pair      |
| **Usage Pattern** | Generic mapping service              | Specialized mapping service                |

### When to Use IMapper

Use the generic `IMapper` interface when you need:

1. **Dynamic Type Mapping**: When source and destination types are determined at runtime
2. **Generic Services**: Building reusable services that work with multiple type combinations
3. **Simplified Registration**: When you want a single mapper service for all mappings
4. **Flexibility**: When you need to map various type combinations without knowing them at compile time

**Example Scenario:**

```csharp
public class GenericDataService
{
    private readonly IMapper _mapper;

    public GenericDataService(IMapper mapper)
    {
        _mapper = mapper;
    }

    // Can map any type combination
    public TDto ConvertToDto<TEntity, TDto>(TEntity entity)
    {
        return _mapper.Map<TEntity, TDto>(entity);
    }
}
```

### When to Use IMapper&lt;TSource, TDestination&gt;

Use the specific `IMapper<TSource, TDestination>` interface when you need:

1. **Maximum Performance**: Direct method calls without service resolution overhead
2. **Compile-Time Safety**: Strong typing prevents mapping to incompatible types
3. **Explicit Dependencies**: Clear indication of exactly which mappers a service requires
4. **Focused Functionality**: Services that work with specific, known type combinations

**Example Scenario:**

```csharp
public class UserService
{
    private readonly IMapper<User, UserDto> _userMapper;
    private readonly IMapper<UserDto, User> _userDtoMapper;

    public UserService(
        IMapper<User, UserDto> userMapper,
        IMapper<UserDto, User> userDtoMapper)
    {
        _userMapper = userMapper;
        _userDtoMapper = userDtoMapper;
    }

    // Compile-time safe, high-performance mapping
    public UserDto GetUserDto(User user)
    {
        return _userMapper.Map(user);
    }

    public User CreateUser(UserDto dto)
    {
        return _userDtoMapper.Map(dto);
    }
}
```

## ServiceProviderMapper

The `ServiceProviderMapper` class provides a default implementation of `IMapper` that resolves specific mappers using dependency injection:

```csharp
public sealed class ServiceProviderMapper(IServiceProvider serviceProvider) : IMapper
{
    public TDestination? Map<TSource, TDestination>(TSource? source)
    {
        if (source is null)
            return default;

        var mapper = serviceProvider.GetRequiredService<IMapper<TSource, TDestination>>();
        return mapper.Map(source);
    }

    public void Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper<TSource, TDestination>>();
        mapper.Map(source, destination);
    }

    public IQueryable<TDestination> ProjectTo<TSource, TDestination>(IQueryable<TSource> source)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper<TSource, TDestination>>();
        return mapper.ProjectTo(source);
    }
}
```

**Key Features:**

- **Service Resolution**: Automatically resolves the appropriate `IMapper<TSource, TDestination>` from the service provider
- **Null Safety**: Handles null source objects gracefully
- **Type Safety**: Leverages the type system to ensure mapping compatibility
- **Performance**: Delegates to specific mappers for optimal performance

## Source-Generated Mapping with MapperProfile

The `MapperProfile<TSource, TDestination>` abstract class combined with the `[GenerateMapper]` attribute enables the source generator to emit strongly-typed mapping implementations at compile time.

### How It Works

1. Create a **partial class** that inherits from `MapperProfile<TSource, TDestination>`
2. Apply the `[GenerateMapper]` attribute
3. Optionally override `ConfigureMapping()` to customize property mappings
4. The source generator automatically implements `Map` and `ProjectTo` methods

The generator automatically maps properties that share a common name and compatible type between source and destination. Custom expressions, constant values, and ignored properties are configured via the `ConfigureMapping` method.

### Creating Source-Generated Mappers

#### Basic Mapper (Auto-Mapped Properties)

When source and destination types share the same property names, no configuration is needed:

```csharp
[GenerateMapper]
public partial class PersonRecordToModelMapper : MapperProfile<PersonRecord, PersonModel>;
```

The generator matches properties by name and type automatically.

#### Mapper with Custom Property Mappings

Override `ConfigureMapping()` to define custom source expressions, constant values, or ignored properties:

```csharp
[GenerateMapper]
public partial class UserToUserDtoMapper : MapperProfile<User, UserDto>
{
    protected override void ConfigureMapping(MappingBuilder<User, UserDto> mapping)
    {
        mapping.Property(d => d.FullName).From(s => s.FirstName + " " + s.LastName);
        mapping.Property(d => d.Age).From(s => DateTime.Now.Year - s.BirthDate.Year);
        mapping.Property(d => d.DepartmentName).From(s => s.Department!.Name);
        mapping.Property(d => d.AddressCount).From(s => s.Addresses.Count());
    }
}
```

### MappingBuilder API

The `MappingBuilder<TSource, TDestination>` provides a fluent API for configuring property mappings. The method body is parsed as syntax at compile time by the source generator — it is never executed at runtime.

#### Property().From() — Custom Source Expression

Map a destination property from a custom source expression:

```csharp
mapping.Property(d => d.FullName).From(s => s.FirstName + " " + s.LastName);
mapping.Property(d => d.Total).From(s => s.Price * s.Quantity);
mapping.Property(d => d.DepartmentName).From(s => s.Department!.Name);
mapping.Property(d => d.AddressCount).From(s => s.Addresses.Count());
```

#### Property().Value() — Constant Value

Assign a constant value to a destination property:

```csharp
mapping.Property(d => d.Source).Value("ImportedData");
mapping.Property(d => d.IsActive).Value(true);
```

#### Property().Ignore() — Skip Property

Exclude a destination property from mapping:

```csharp
mapping.Property(d => d.InternalNotes).Ignore();
mapping.Property(d => d.Department).Ignore();
```

### ConfigureMapping Guidelines

Because the `ConfigureMapping` method body is only parsed as syntax by the source generator, it must contain only `MappingBuilder` configuration calls. Arbitrary runtime logic such as conditionals, loops, or service calls is not supported and will be silently ignored by the generator.

### Custom Expression Guidelines

When writing custom `From()` expressions, follow these guidelines for optimal performance and compatibility with `ProjectTo` query translation:

#### Supported Patterns

1. **Simple Property Mapping**:

   ```csharp
   mapping.Property(d => d.Name).From(s => s.Name);
   ```

2. **String Concatenation**:

   ```csharp
   mapping.Property(d => d.FullName).From(s => s.FirstName + " " + s.LastName);
   ```

3. **Method Calls**:

   ```csharp
   mapping.Property(d => d.UpperName).From(s => s.Name.ToUpper());
   ```

4. **Conditional Logic**:

   ```csharp
   mapping.Property(d => d.IsValid).From(s => s.Status == EntityStatus.Active);
   ```

5. **Navigation Properties**:

   ```csharp
   mapping.Property(d => d.DepartmentName).From(s => s.Department!.Name);
   ```

6. **Collection Aggregates**:

   ```csharp
   mapping.Property(d => d.ItemCount).From(s => s.Items.Count());
   mapping.Property(d => d.TotalAmount).From(s => s.Orders.Sum(o => o.Amount));
   ```

#### Important Restrictions

The custom expressions are emitted into both direct mapping code and expression trees for `ProjectTo`. Understanding these restrictions helps ensure your mappers work correctly across all scenarios.

1. **Use LINQ Methods for Aggregates**: Use `Count()`, `Sum()`, etc. instead of collection properties

   **Why this restriction exists**: Collection properties like `.Count` are not expression-tree compatible and cannot be translated by query providers. LINQ methods like `.Count()` are specifically designed for expression tree translation and will generate optimized SQL aggregates.

   ```csharp
   // ❌ Don't use - Property access, not translatable to SQL
   mapping.Property(d => d.ItemCount).From(s => s.Items.Count);
   
   // ✅ Use instead - Method call, translates to SQL COUNT()
   mapping.Property(d => d.ItemCount).From(s => s.Items.Count());
   ```

   **Expression impact**: When this expression is used in `ProjectTo`, the LINQ method translates to:
   
   ```sql
   (SELECT COUNT(*) FROM [Items] WHERE [Items].[ParentId] = [Source].[Id])
   ```

2. **Avoid Complex Method Chains**: Keep expressions simple for better SQL translation

   **Why this restriction exists**: While not strictly forbidden, complex method chains can result in inefficient SQL or may not translate at all, forcing client-side evaluation.

   ```csharp
   // ❌ Potential issues - Complex chain may not optimize well
   mapping.Property(d => d.FullAddress)
       .From(s => s.Addresses.Where(a => a.IsPrimary).FirstOrDefault()!.Street.ToUpper());
   
   // ✅ Better approach - Simplified expression
   mapping.Property(d => d.PrimaryStreet)
       .From(s => s.Addresses.Where(a => a.IsPrimary).Select(a => a.Street).FirstOrDefault());
   ```

### Registration Example

Register your source-generated mappers in the dependency injection container:

```csharp
services.AddSingleton<IMapper<User, UserDto>, UserToUserDtoMapper>();
services.AddSingleton<IMapper<UserDto, User>, UserDtoToUserMapper>();
services.AddSingleton<IMapper, ServiceProviderMapper>();
```

## Usage Examples

### Creating New Objects

```csharp
public class UserService
{
    private readonly IMapper _mapper;

    public UserService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public UserDto GetUserDto(User user)
    {
        return _mapper.Map<User, UserDto>(user);
    }
}
```

### Updating Existing Objects

```csharp
public void UpdateUserFromDto(UserDto dto, User existingUser)
{
    _mapper.Map(dto, existingUser);
}
```

### Query Projection

```csharp
public async Task<List<UserDto>> GetUserDtosAsync()
{
    return await _dbContext.Users
        .ProjectTo<User, UserDto>(_mapper)
        .ToListAsync();
}
```

### Using Specific Mappers

```csharp
public class UserService
{
    private readonly IMapper<User, UserDto> _userMapper;

    public UserService(IMapper<User, UserDto> userMapper)
    {
        _userMapper = userMapper;
    }

    public UserDto GetUserDto(User user)
    {
        return _userMapper.Map(user);
    }
}
```

## Performance Considerations

### Singleton Registration

For optimal performance, register mapper implementations as singletons:

```csharp
services.AddSingleton<IMapper<User, UserDto>, UserToUserDtoMapper>();
```

Since all mapping code is generated at compile time, mapper instances are stateless and safe to reuse as singletons.

### Zero Reflection Overhead

The source generator emits direct property assignments — no reflection, no expression compilation, and no runtime code generation. This makes mappers fully compatible with Native AOT.

### Query Translation

When using `ProjectTo` with Entity Framework or other query providers:

1. The generated expression tree is translated to SQL
2. Only required data is loaded from the database
3. The projection expression is cached in a static field to avoid per-call allocations

### Memory Efficiency

- Generated code provides near-native performance
- No reflection overhead during mapping operations
- Minimal memory allocation for mapping operations
- Static expression caching eliminates repeated allocations

## Advanced Scenarios

### Mapping to Records

The source generator supports mapping to record types with `init` properties:

```csharp
public record PersonRecord
{
    public int Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public int Age { get; init; }
    public string? DepartmentName { get; init; }
    public int AddressCount { get; init; }
}

[GenerateMapper]
public partial class PersonToRecordMapper : MapperProfile<Person, PersonRecord>
{
    protected override void ConfigureMapping(MappingBuilder<Person, PersonRecord> mapping)
    {
        mapping.Property(d => d.FullName).From(p => p.FirstName + " " + p.LastName);
        mapping.Property(d => d.Age).From(p => DateTime.Now.Year - p.BirthDate.Year);
        mapping.Property(d => d.DepartmentName).From(p => p.Department!.Name);
        mapping.Property(d => d.AddressCount).From(p => p.Addresses.Count());
    }
}
```

**Note**: Record types with all `init` or read-only properties support creation of new instances but not updating existing ones. The generated `Map(source, destination)` overload will throw `NotSupportedException` in this case.

### Mapping with Constructor Parameters

The generator supports destination types that use primary constructors:

```csharp
public class PersonConstructorModel(
    int id, string firstName, string lastName, string email,
    string fullName, int age, string? departmentName, int addressCount)
{
    public int Id { get; } = id;
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
    public string Email { get; } = email;
    public string FullName { get; } = fullName;
    public int Age { get; } = age;
    public string? DepartmentName { get; } = departmentName;
    public int AddressCount { get; } = addressCount;
}

[GenerateMapper]
public partial class PersonToConstructorModelMapper : MapperProfile<Person, PersonConstructorModel>
{
    protected override void ConfigureMapping(MappingBuilder<Person, PersonConstructorModel> mapping)
    {
        mapping.Property(d => d.FullName).From(p => p.FirstName + " " + p.LastName);
        mapping.Property(d => d.Age).From(p => DateTime.Now.Year - p.BirthDate.Year);
        mapping.Property(d => d.DepartmentName).From(p => p.Department!.Name);
        mapping.Property(d => d.AddressCount).From(p => p.Addresses.Count());
    }
}
```

### Handling Complex Scenarios

For mappings that cannot be expressed as simple property expressions, consider:

1. **Pre-processing**: Transform data before mapping
2. **Post-processing**: Transform data after mapping
3. **Custom implementations**: Implement `IMapper<TSource, TDestination>` directly
4. **Composite mappers**: Chain multiple mappers together

### Error Handling

The framework provides clear error messages for common issues:

- **Missing `[GenerateMapper]` attribute**: Calling `Map` or `ProjectTo` on a `MapperProfile` without the attribute throws `NotImplementedException` with a descriptive message
- **Read-only destination**: When all destination properties are read-only, the `Map(source, destination)` overload throws `NotSupportedException`
- **Service resolution failures**: When mappers are not registered in the DI container

## Best Practices

1. **Always use `partial` classes** with the `[GenerateMapper]` attribute
2. **Register mappers as singletons** for performance
3. **Use `ConfigureMapping`** only for properties that cannot be auto-mapped by name
4. **Use specific mapper interfaces** (`IMapper<TSource, TDestination>`) when possible for better performance
5. **Keep custom expressions simple** for compatibility with `ProjectTo` query translation
6. **Test your mappers** thoroughly, especially edge cases like null values
7. **Use `ProjectTo`** for query scenarios to improve database performance
