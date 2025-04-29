# MongoDB Handlers

Mongo DB handlers for the base Commands and Queries

## Installation

```powershell
Install-Package Arbiter.CommandQuery.MongoDB
```

OR

```shell
dotnet add package Arbiter.CommandQuery.MongoDB
```

## Usage

Register via dependency injection

```csharp
// Add MongoDB Repository services
services.AddMongoRepository("Tracker");

// Register Command Query services
services.AddCommandQuery();

// Implement and register IMapper
services.AddSingleton<IMapper, MyMapper>();

// Implement and register IValidator
services.AddSingleton<IValidator, MyValidator>();

// Register MongoDB handlers for each Entity
services.AddEntityQueries<IMongoEntityRepository<Product>, Product, string, ProductReadModel>();
services.AddEntityCommands<IMongoEntityRepository<Product>, Product, string, ProductReadModel, ProductCreateModel, ProductUpdateModel>();
```
