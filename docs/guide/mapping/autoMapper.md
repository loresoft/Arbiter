# AutoMapper Implementation

AutoMapper is a popular object-to-object mapping library that simplifies the task of mapping between different object types. The Arbiter framework supports AutoMapper through adapter implementations that integrate seamlessly with the existing mapping infrastructure.

## Overview

AutoMapper provides several benefits when integrated with the Arbiter framework:

- **Convention-based mapping**: Automatically maps properties with matching names
- **Profile-based configuration**: Organize mapping configurations into reusable profiles
- **Runtime flexibility**: Configure mappings dynamically at runtime
- **Extensive customization**: Support for custom value resolvers, type converters, and conditional mapping
- **Query projection**: Supports projecting LINQ queries for Entity Framework integration

## Installation

Add the AutoMapper package to your project:

```xml
<PackageReference Include="AutoMapper" Version="14.0.0" />
```

## Basic Implementation

### Creating AutoMapper Profiles

AutoMapper uses profiles to organize mapping configurations. Create profiles that inherit from `AutoMapper.Profile`:

```csharp
using AutoMapper;
using Entities = MyProject.Data.Entities;
using Models = MyProject.Domain.Models;

namespace MyProject.Domain.Mapping;

public partial class PriorityProfile : Profile
{
    public PriorityProfile()
    {
        // Entity to ReadModel mapping
        CreateMap<Entities.Priority, Models.PriorityReadModel>();

        // CreateModel to Entity mapping
        CreateMap<Models.PriorityCreateModel, Entities.Priority>();

        // Entity to UpdateModel mapping
        CreateMap<Entities.Priority, Models.PriorityUpdateModel>();

        // UpdateModel to Entity mapping
        CreateMap<Models.PriorityUpdateModel, Entities.Priority>();

        // ReadModel to CreateModel mapping
        CreateMap<Models.PriorityReadModel, Models.PriorityCreateModel>();

        // ReadModel to UpdateModel mapping
        CreateMap<Models.PriorityReadModel, Models.PriorityUpdateModel>();
    }
}
```

### Creating AutoMapper Adapters

To integrate AutoMapper with the Arbiter framework, create adapter classes that implement the Arbiter mapping interfaces:

```csharp
using System.Diagnostics.CodeAnalysis;
using Arbiter.CommandQuery.Definitions;

namespace MyProject.Adapters;

internal class AutoMapperAdapter(AutoMapper.IMapper mapper) : IMapper
{
    [return: NotNullIfNotNull(nameof(source))]
    public TDestination? Map<TSource, TDestination>(TSource? source)
    {
        if (source is null)
            return default;

        return mapper.Map<TSource, TDestination>(source);
    }

    public void Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        mapper.Map(source, destination);
    }

    public IQueryable<TDestination> ProjectTo<TSource, TDestination>(IQueryable<TSource> source)
    {
        return mapper.ProjectTo<TDestination>(source);
    }
}
```

### Specific Type Adapters

For better performance and type safety, you can create specific adapters for individual type mappings:

```csharp
using System.Diagnostics.CodeAnalysis;
using Arbiter.CommandQuery.Definitions;

namespace MyProject.Mapping;

public class PriorityAutoMapper(AutoMapper.IMapper mapper) : IMapper<Priority, PriorityReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public PriorityReadModel? Map(Priority? source)
    {
        if (source is null)
            return default;

        return mapper.Map<Priority, PriorityReadModel>(source);
    }

    public void Map(Priority source, PriorityReadModel destination)
    {
        mapper.Map(source, destination);
    }

    public IQueryable<PriorityReadModel> ProjectTo(IQueryable<Priority> source)
    {
        return mapper.ProjectTo<PriorityReadModel>(source);
    }
}
```

## Service Registration

### Basic Registration

Register AutoMapper in your dependency injection container:

```csharp
using Microsoft.Extensions.DependencyInjection;

// In Program.cs or Startup.cs
services.AddAutoMapper(typeof(Program).Assembly);

// Register the AutoMapper adapter
services.AddSingleton<IMapper, AutoMapperAdapter>();

// Or register specific mappers
services.AddSingleton<IMapper<Priority, PriorityReadModel>, PriorityAutoMapper>();
```

### Advanced Registration with Configuration

For more complex scenarios, configure AutoMapper explicitly:

```csharp
using AutoMapper;

// Configure AutoMapper
services.AddSingleton<IMapper>(provider =>
{
    var configuration = new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<PriorityProfile>();
        cfg.AddProfile<UserProfile>();
        cfg.AddProfile<TaskProfile>();
        
        // Add custom configurations
        cfg.AllowNullCollections = true;
        cfg.AllowNullDestinationValues = true;
    });
    
    return configuration.CreateMapper();
});

// Register Arbiter adapter
services.AddSingleton<IMapper, AutoMapperAdapter>();
```

## Advanced Configurations

### Custom Property Mapping

AutoMapper provides powerful customization options through profiles:

```csharp
public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => DateTime.Now.Year - src.BirthDate.Year))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status == UserStatus.Active))
            .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null));

        CreateMap<UserDto, User>()
            .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => DateTime.Now.AddYears(-src.Age)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive ? UserStatus.Active : UserStatus.Inactive))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}
```

### Conditional Mapping

Map properties conditionally based on source values:

```csharp
public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Items.Sum(i => i.Price * i.Quantity)))
            .ForMember(dest => dest.Status, opt => opt.Condition(src => src.Status != null))
            .ForMember(dest => dest.ShippingAddress, opt => opt.Condition(src => src.RequiresShipping));
    }
}
```

## Usage Examples

### Using Generic AutoMapper Adapter

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

    public void UpdateUserFromDto(UserDto dto, User existingUser)
    {
        _mapper.Map(dto, existingUser);
    }

    public async Task<List<UserDto>> GetUserDtosAsync()
    {
        return await _dbContext.Users
            .ProjectTo<User, UserDto>(_mapper)
            .ToListAsync();
    }
}
```

### Using Specific AutoMapper Adapters

```csharp
public class PriorityService
{
    private readonly IMapper<Priority, PriorityReadModel> _priorityMapper;

    public PriorityService(IMapper<Priority, PriorityReadModel> priorityMapper)
    {
        _priorityMapper = priorityMapper;
    }

    public PriorityReadModel GetPriorityReadModel(Priority priority)
    {
        return _priorityMapper.Map(priority);
    }

    public async Task<List<PriorityReadModel>> GetPrioritiesAsync()
    {
        return await _dbContext.Priorities
            .ProjectTo(_priorityMapper)
            .ToListAsync();
    }
}
```

## Performance Considerations

### Benchmark Results

Based on the Arbiter framework benchmarks, AutoMapper performance compared to other mapping solutions:

| Method             | Mean        | Ratio    | Allocated |
| ------------------ | ----------- | -------- | --------- |
| Manual Mapping     | 15.2 ns     | 1.00     | -         |
| Mapperly           | 16.8 ns     | 1.11     | -         |
| Arbiter MapperBase | 24.3 ns     | 1.60     | -         |
| **AutoMapper**     | **89.4 ns** | **5.88** | **80 B**  |

### Performance Optimization Tips

1. **Use Compiled Mappings**: AutoMapper compiles mappings for better performance
2. **Avoid Complex Expressions**: Keep mapping expressions simple for better performance
3. **Use Specific Mappers**: Create specific mapper adapters for frequently used mappings
4. **Configure Once**: Create mapper configuration once and reuse across the application
5. **Minimize Allocations**: Use projection for query scenarios to reduce memory usage

### When to Use AutoMapper

AutoMapper is ideal when:

- **Complex Object Graphs**: You need to map complex nested objects
- **Dynamic Mappings**: Mapping requirements change frequently
- **Convention-based Mapping**: Most properties can be mapped by convention
- **Existing AutoMapper Investment**: You already have extensive AutoMapper configurations
- **Third-party Integration**: Working with libraries that provide AutoMapper profiles

## Comparison with Other Mapping Solutions

| Aspect                 | AutoMapper                          | MapperBase                          | Mapperly                             |
| ---------------------- | ----------------------------------- | ----------------------------------- | ------------------------------------ |
| **Performance**        | Good (runtime optimized)            | Very Good (compiled expressions)    | Excellent (source generated)         |
| **Setup Complexity**   | Medium (profiles and configuration) | Medium (expression writing)         | Low (attributes only)                |
| **Flexibility**        | Excellent (extensive customization) | Good (expression control)           | Good (attributes and custom methods) |
| **Runtime Overhead**   | Higher (reflection-based)           | Lower (compiled expressions)        | Minimal (compile-time generated)     |
| **Query Translation**  | Good (ProjectTo support)            | Excellent (full expression support) | Good (basic LINQ support)            |
| **Convention Support** | Excellent                           | None                                | Basic                                |
| **Complex Scenarios**  | Excellent                           | Good                                | Limited                              |
| **Learning Curve**     | Medium                              | Low                                 | Low                                  |

## Best Practices

### 1. Organize Profiles by Domain

```csharp
// Group related mappings in domain-specific profiles
public class UserProfile : Profile { }
public class OrderProfile : Profile { }
public class ProductProfile : Profile { }
```

### 2. Use Configuration Validation

```csharp
services.AddSingleton<IMapper>(provider =>
{
    var configuration = new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<UserProfile>();
        // Add validation in development
        cfg.AssertConfigurationIsValid();
    });
    
    return configuration.CreateMapper();
});
```

### 3. Handle Null Values Explicitly

```csharp
public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Department, opt => opt.MapFrom(src => 
                src.Department != null ? src.Department.Name : "No Department"));
    }
}
```

### 4. Use Projection for Queries

```csharp
// Use ProjectTo for database queries
var users = await dbContext.Users
    .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
    .ToListAsync();
```
