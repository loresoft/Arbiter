---
title: Code Generation
description: Code Generation with EntityFrameworkCore.Generator
---

# Code Generation

Arbiter provides code generation templates that work with [EntityFrameworkCore.Generator](https://github.com/loresoft/EntityFrameworkCore.Generator) to automatically create boilerplate code for your entities. This helps maintain consistency and reduces manual coding for common patterns.

## Overview

The code generation system uses C# script files (`.csx`) as templates that are processed by EntityFrameworkCore.Generator. These templates generate various types of code based on your Entity Framework models:

- **Service Registration** - Dependency injection setup
- **Mappers** - Object mapping between entities and models  
- **Endpoints** - Minimal API endpoints for CRUD operations
- **JSON Context** - System.Text.Json serialization setup
- **Entity Store** - Client-side state management

## Dependencies

The generated templates use attributes for dependency injection registration that require [Injectio](https://github.com/loresoft/Injectio) to be installed in your project. Injectio provides source generation for dependency injection registration using attributes like `[RegisterServices]`, `[RegisterSingleton]`, `[RegisterScoped]`, etc.

To install Injectio, add the following package reference to your project:

```xml
<PackageReference Include="Injectio" Version="5.0.0" PrivateAssets="All" />
```

## Template Files

### DomainServiceRegistration.csx

Creates dependency injection registration for each entity. This template generates service registration classes that configure the required services for entity queries and commands.

**Purpose:**

- Registers `EntityQueries` for read operations
- Registers `EntityCommands` for create, update, delete operations
- Uses the `[RegisterServices]` attribute for automatic discovery

**Generated Code Example:**

```csharp
public static class UserServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddEntityQueries<TrackerContext, User, int, UserReadModel>();
        services.AddEntityCommands<TrackerContext, User, int, UserReadModel, UserCreateModel, UserUpdateModel>();
    }
}
```

### EntityMapper.csx

Creates [Riok.Mapperly](https://github.com/riok/mapperly) mappers for each entity. This template generates high-performance, compile-time mappers between different model types.

**Purpose:**

- Maps between entities and read/create/update models
- Maps between different model types (e.g., read to update)
- Excludes specified properties from mapping
- Uses compile-time source generation for performance

**Generated Code Example:**

```csharp
[Mapper]
[RegisterSingleton<IMapper<Entities.User, Models.UserReadModel>>]
internal sealed partial class UserToUserReadModelMapper
    : IMapper<Entities.User, Models.UserReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.UserReadModel? Map(
        Entities.User? source);

    public partial void Map(
        Entities.User source,
        Models.UserReadModel destination);

    public partial IQueryable<Models.UserReadModel> ProjectTo(
        IQueryable<Entities.User> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.UserCreateModel, Entities.User>>]
internal sealed partial class UserCreateModelToUserMapper
    : IMapper<Models.UserCreateModel, Entities.User>
{
    [return: NotNullIfNotNull(nameof(source))]
    [MapperIgnoreTarget(nameof(Entities.User.RowVersion))]
    public partial Entities.User? Map(
        Models.UserCreateModel? source);

    [MapperIgnoreTarget(nameof(Entities.User.RowVersion))]
    public partial void Map(
        Models.UserCreateModel source,
        Entities.User destination);

    public partial IQueryable<Entities.User> ProjectTo(
        IQueryable<Models.UserCreateModel> source);
}
```

### EntityMapping.csx

Creates Arbiter `MapperBase` mappers for each entity. This template generates mapping classes that inherit from Arbiter's mapping infrastructure.

**Purpose:**

- Provides custom mapping logic when needed
- Supports manual property mapping configuration
- Integrates with Arbiter's mapping system
- Allows for complex mapping scenarios

**Generated Code Example:**

```csharp
[RegisterSingleton<IMapper<Entities.User, Models.UserReadModel>>]
internal sealed class UserToUserReadModelMapper
    : MapperBase<Entities.User, Models.UserReadModel>
{
    protected override Expression<Func<Entities.User, Models.UserReadModel>> CreateMapping()
    {
        return source => new Models.UserReadModel
        {
            Id = source.Id,
            DisplayName = source.DisplayName,
            EmailAddress = source.EmailAddress,
            IsDeleted = source.IsDeleted,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion,
        };
    }
}

[RegisterSingleton<IMapper<Models.UserCreateModel, Entities.User>>]
internal sealed class UserCreateModelToUserMapper
    : MapperBase<Models.UserCreateModel, Entities.User>
{
    protected override Expression<Func<Models.UserCreateModel, Entities.User>> CreateMapping()
    {
        return source => new Entities.User
        {
            Id = source.Id,
            DisplayName = source.DisplayName,
            EmailAddress = source.EmailAddress,
            IsDeleted = source.IsDeleted,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
        };
    }
}
```

### EntityEndpoint.csx

Creates Minimal API endpoints for each entity. This template generates endpoint classes that provide REST API functionality.

**Purpose:**

- Creates CRUD endpoints (GET, POST, PUT, DELETE)
- Integrates with Arbiter's mediation pattern
- Supports both query-only and full CRUD operations
- Uses automatic service registration

**Generated Code Example:**

```csharp
[RegisterSingleton<IEndpointRoute>(Duplicate = DuplicateStrategy.Append)]
public class UserEndpoint : EntityCommandEndpointBase<string, UserReadModel, UserReadModel, UserCreateModel, UserUpdateModel>
{
    public UserEndpoint(ILoggerFactory loggerFactory)
        : base(loggerFactory, "User")
    { 

    }
}
```

### DomainJsonContext.csx

Creates System.Text.Json serialization context for all entity models. This template generates a JSON source generation context that provides optimized serialization for your models.

**Purpose:**

- Generates `JsonSerializerContext` for all entity models
- Includes support for queries, commands, and result types
- Uses source generation for optimal performance
- Configures JSON naming policies and options

**Generated Code Example:**

```csharp
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(EntityIdentifierQuery<int, UserReadModel>))]
[JsonSerializable(typeof(UserReadModel))]
[JsonSerializable(typeof(UserCreateModel))]
[JsonSerializable(typeof(EntityCreateCommand<UserCreateModel, UserReadModel>))]
public partial class DomainJsonContext : JsonSerializerContext
{
}
```

## Configuration

### generation.yml

The code generation is configured through a `generation.yml` file that specifies how and where code should be generated.

**Basic Structure:**

```yaml
project:
  namespace: 'YourProject'
  directory: .\
  nullable: true
  fileScopedNamespace: true

database:
  provider: SqlServer
  connectionString: 'Your connection string'

# Entity and model configuration
data:
  entity:
    namespace: '{Project.Namespace}.Data.Entities'
    directory: '{Project.Directory}\Data\Entities'
    baseClass: 'IHaveIdentifier<int>, ITrackCreated, ITrackUpdated'

model:
  read:
    generate: true
    name: '{Entity.Name}ReadModel'
    baseClass: 'EntityReadModel'
  create:
    generate: true
    name: '{Entity.Name}CreateModel'
    baseClass: 'EntityCreateModel'
  update:
    generate: true
    name: '{Entity.Name}UpdateModel'
    baseClass: 'EntityUpdateModel'

# Script configuration for templates
script:
  context:
    - templatePath: '..\..\..\..\templates\DomainJsonContext.csx'
      fileName: 'DomainJsonContext.cs'
      namespace: '{Project.Namespace}.Domain'
      directory: '{Project.Namespace}\Domain'
      merge: true

  entity:
    - templatePath: '..\..\templates\DomainServiceRegistration.csx'
      fileName: '{Entity.Name}ServiceRegistration.cs'
      namespace: '{Project.Namespace}.Domain'
      directory: '{Project.Directory}\Domain\{Entity.Name}'
      parameters:
        keyType: int

    - templatePath: '..\..\templates\EntityMapper.csx'
      fileName: '{Entity.Name}Mapper.cs'
      namespace: '{Project.Namespace}.Domain.Mapping'
      directory: '{Project.Directory}\Domain\{Entity.Name}\Mapping'
      parameters:
        excludeDomain: false
        excludeEntity: false

    - templatePath: '..\..\templates\EntityMapping.csx'
      fileName: '{Entity.Name}Mapping.cs'
      namespace: '{Project.Namespace}.Domain.Mapping'
      directory: '{Project.Directory}\Domain\{Entity.Name}\Mapping'
      merge: true
      parameters:
        readMapping: 'Id,Created,CreatedBy,Updated,UpdatedBy,RowVersion'
        createMapping: 'Id,Created,CreatedBy,Updated,UpdatedBy'
        updateMapping: 'Updated,UpdatedBy,RowVersion'

    - templatePath: '..\..\templates\EntityEndpoint.csx'
      fileName: '{Entity.Name}Endpoint.cs'
      namespace: '{Project.Namespace}.Api.Endpoints'
      directory: '{Project.Directory}\Api\Endpoints'
      parameters:
        keyType: int
```

### Template Parameters

Each template accepts parameters that customize the generated code:

**Common Parameters:**

- `keyType` - The type of the entity's primary key (e.g., `int`, `Guid`)
- `excludeDomain` - Skip domain model mapping
- `excludeEntity` - Skip entity mapping

**EntityMapping.csx Parameters:**

- `readMapping` - Properties to include in read model mapping
- `createMapping` - Properties to include in create model mapping  
- `updateMapping` - Properties to include in update model mapping

## Usage

1. **Install EntityFrameworkCore.Generator:**

   ```bash
   dotnet tool install -g EntityFrameworkCore.Generator
   ```

2. **Create generation.yml configuration file** in your project root

3. **Run the generator:**

   ```bash
   efg generate
   ```

4. **Generated files** will be created according to your configuration

## Integration with Arbiter

The generated code integrates seamlessly with Arbiter's patterns:

- **Service Registration** - Uses `[RegisterServices]` for automatic DI setup
- **Mediation** - Endpoints use `IMediator` for command/query handling
- **Mapping** - Integrates with both Mapperly and Arbiter's mapping system
- **CRUD Operations** - Leverages Arbiter's entity command/query infrastructure

This approach ensures consistency across your application while reducing boilerplate code and maintaining type safety.
