---
title: Mapping Process
description: Flexible and high-performance mapping system for transforming objects from one type to another
---

# Mapping Process

The Arbiter framework provides a flexible and high-performance mapping system that supports transforming objects from one type to another. The mapping system is built around several key components that work together to provide both compile-time safety and runtime performance.

## Core Mapping Interfaces

### IMapper

The `IMapper` interface provides a generic contract for object mapping operations:

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

This interface is implemented by specific mapper classes and registered in the dependency injection container for type-safe resolution.

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

## MapperBase&lt;TSource, TDestination&gt;

The `MapperBase<TSource, TDestination>` abstract class provides a high-performance base implementation for creating custom mappers:

```csharp
public abstract class MapperBase<TSource, TDestination> : IMapper<TSource, TDestination>
{
    protected abstract Expression<Func<TSource, TDestination>> CreateMapping();
}
```

**Key Features:**

- **Expression-Based**: Uses LINQ expressions for compile-time mapping definitions
- **Performance Optimized**: Compiles expressions at construction time for fast runtime execution
- **Query Translation**: Supports `ProjectTo` for Entity Framework and other query providers
- **Singleton-Ready**: Designed for singleton registration in dependency injection containers

### Creating Custom Mappers

To create a custom mapper, inherit from `MapperBase<TSource, TDestination>` and implement the `CreateMapping()` method:

```csharp
public class UserToUserDtoMapper : MapperBase<User, UserDto>
{
    protected override Expression<Func<User, UserDto>> CreateMapping()
    {
        return user => new UserDto
        {
            Id = user.Id,
            FullName = user.FirstName + " " + user.LastName,
            Email = user.Email,
            IsActive = user.Status == UserStatus.Active,
            CreatedDate = user.CreatedAt.Date,
            Department = user.Department != null ? user.Department.Name : null,
            OrderCount = user.Orders.Count(),
            TotalOrderAmount = user.Orders.Sum(o => o.Amount)
        };
    }
}
```

### Mapping Expression Guidelines

When implementing `CreateMapping()`, follow these guidelines for optimal performance and compatibility:

#### Supported Patterns

1. **Simple Property Mapping**:

   ```csharp
   Name = source.Name
   ```

2. **String Concatenation**:

   ```csharp
   FullName = source.FirstName + " " + source.LastName
   ```

3. **Method Calls**:

   ```csharp
   UpperName = source.Name.ToUpper()
   ```

4. **Conditional Logic**:

   ```csharp
   IsValid = source.Status == EntityStatus.Active
   ```

5. **Explicit Null Checks**:

   ```csharp
   Phone = source.Contact != null ? source.Contact.PhoneNumber : null
   ```

6. **Collection Aggregates**:

   ```csharp
   ItemCount = source.Items.Count()
   TotalAmount = source.Orders.Sum(o => o.Amount)
   ```

#### Important Restrictions

The mapping expressions have certain limitations due to how they are processed and translated. Understanding these restrictions helps ensure your mappers work correctly across all scenarios.

1. **No Null-Conditional Operators**: Use explicit null checks instead of `?.` operator

   **Why this restriction exists**: The null-conditional operator (`?.`) are not expression-tree compatible .

   ```csharp
   // ❌ Don't use - not expression-tree compatible
   Phone = source.Contact?.PhoneNumber
   
   // ✅ Use instead - translates to proper SQL NULL checks
   Phone = source.Contact != null ? source.Contact.PhoneNumber : null
   ```

   **Expression impact**: When this expression is used in `ProjectTo`, the explicit null check translates to:
   
   ```sql
   CASE WHEN [Contact] IS NOT NULL THEN [Contact].[PhoneNumber] ELSE NULL END
   ```

2. **Use LINQ Methods for Aggregates**: Use `Count()`, `Sum()`, etc. instead of collection properties

   **Why this restriction exists**: Collection properties like `.Count` are not expression-tree compatible and cannot be translated by query providers. LINQ methods like `.Count()` are specifically designed for expression tree translation and will generate optimized SQL aggregates.

   ```csharp
   // ❌ Don't use - Property access, not translatable to SQL
   ItemCount = source.Items.Count
   
   // ✅ Use instead - Method call, translates to SQL COUNT()
   ItemCount = source.Items.Count()
   ```

   **Expression impact**: When this expression is used in `ProjectTo`, the LINQ method translates to:
   
   ```sql
   (SELECT COUNT(*) FROM [Items] WHERE [Items].[ParentId] = [Source].[Id])
   ```

3. **Avoid Complex Method Chains**: Keep expressions simple for better SQL translation

   **Why this restriction exists**: While not strictly forbidden, complex method chains can result in inefficient SQL or may not translate at all, forcing client-side evaluation.

   ```csharp
   // ❌ Potential issues - Complex chain may not optimize well
   FullAddress = source.Addresses.Where(a => a.IsPrimary).FirstOrDefault()?.Street?.ToUpper()
   
   // ✅ Better approach - Simplified expression with explicit null checks
   PrimaryStreet = source.Addresses.Where(a => a.IsPrimary).Select(a => a.Street).FirstOrDefault()
   ```

4. **No Local Variables or Closures**: All values must come from the source parameter

   **Why this restriction exists**: Expressions must be pure and contain only the source parameter to be serializable and translatable to SQL. Local variables and closures cannot be translated by query providers.

   ```csharp
   // ❌ Don't use - References external variable
   var currentDate = DateTime.Now;
   return source => new UserDto
   {
       Age = currentDate.Year - source.BirthDate.Year // References local variable
   };
   
   // ✅ Use instead - Self-contained expression
   return source => new UserDto
   {
       Age = DateTime.Now.Year - source.BirthDate.Year // Direct reference
   };
   ```

**Performance Note**: Following these restrictions ensures that your mapping expressions will work efficiently in all scenarios - whether creating new objects in memory, updating existing objects, or projecting queries to the database.

### Registration Example

Register your mappers in the dependency injection container:

```csharp
services.RegisterSingleton<IMapper<User, UserDto>, UserToUserDtoMapper>();
services.RegisterSingleton<IMapper<UserDto, User>, UserDtoToUserMapper>();
services.RegisterSingleton<IMapper, ServiceProviderMapper>();
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

For optimal performance, register `MapperBase` implementations as singletons:

```csharp
services.RegisterSingleton<IMapper<User, UserDto>, UserToUserDtoMapper>();
```

The mapping expressions are compiled once at construction time and reused for all mapping operations.

### Query Translation

When using `ProjectTo` with Entity Framework or other query providers:

1. The mapping expression is translated to SQL
2. Only required data is loaded from the database
3. Complex transformations are performed at the database level

### Memory Efficiency

- Compiled expressions provide near-native performance
- No reflection overhead during mapping operations
- Minimal memory allocation for mapping operations

## Advanced Scenarios

### Mapping to Records

The framework supports mapping to record types:

```csharp
public class PersonToPersonRecordMapper : MapperBase<Person, PersonRecord>
{
    protected override Expression<Func<Person, PersonRecord>> CreateMapping()
    {
        return person => new PersonRecord(
            person.Id,
            person.FirstName,
            person.LastName,
            person.Email,
            person.FirstName + " " + person.LastName,
            DateTime.Now.Year - person.BirthDate.Year,
            person.Department != null ? person.Department.Name : null,
            person.Addresses.Count()
        );
    }
}
```

**Note**: Record types only support creation of new instances, not updating existing ones.

### Handling Complex Scenarios

For mappings that cannot be expressed as simple expressions, consider:

1. **Pre-processing**: Transform data before mapping
2. **Post-processing**: Transform data after mapping
3. **Custom implementations**: Implement `IMapper<TSource, TDestination>` directly
4. **Composite mappers**: Chain multiple mappers together

### Error Handling

The framework provides clear error messages for common issues:

- **Unsupported mapping expressions**: When expressions cannot be converted to assignments
- **Null reference exceptions**: When null checks are missing
- **Service resolution failures**: When mappers are not registered

## Best Practices

1. **Use object initializer syntax** in mapping expressions
2. **Include explicit null checks** for nullable properties
3. **Register mappers as singletons** for performance
4. **Use specific mapper interfaces** when possible for better performance
5. **Avoid complex logic** in mapping expressions - use separate methods instead
6. **Test your mappers** thoroughly, especially edge cases like null values
7. **Use `ProjectTo`** for query scenarios to improve database performance
