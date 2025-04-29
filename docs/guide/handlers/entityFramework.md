# Entity Framework Core Handlers

Entity Framework Core handlers for the base Commands and Queries

## Installation

```powershell
Install-Package Arbiter.CommandQuery.EntityFramework
```

OR

```shell
dotnet add package Arbiter.CommandQuery.EntityFramework
```

## Usage

Register via dependency injection

```csharp
// Add Entity Framework Core services
services.AddDbContext<TrackerContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("TrackerConnection"))
);

// Register Command Query services
services.AddCommandQuery();

// Implement and register IMapper
services.AddSingleton<IMapper, MyMapper>();

// Implement and register IValidator
services.AddSingleton<IValidator, MyValidator>();

// Register Entity Framework Core handlers for each Entity
services.AddEntityQueries<TrackerContext, Product, int, ProductReadModel>();
services.AddEntityCommands<TrackerContext, Product, int, ProductReadModel, ProductCreateModel, ProductUpdateModel>();
```
