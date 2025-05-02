# Command Query

Command Query Responsibility Segregation (CQRS) framework based on mediator pattern

## Command Query Installation

The Arbiter Command Query library is available on nuget.org via package name `Arbiter.CommandQuery`.

To install Arbiter Command Query, run the following command in the Package Manager Console

```powershell
Install-Package Arbiter.CommandQuery
```

OR

```shell
dotnet add package Arbiter.CommandQuery
```

## Command Query Features

- Base commands and queries for common CRUD operations
- Generics base handlers for implementing common CRUD operations
- Common behaviors for audit, caching, soft delete, multi-tenant
- View model to data modal mapping
- Entity Framework Core handlers for common CRUD operations
- MongoDB handlers for common CRUD operations

## Command Query Usage

Register Command Query services via dependency injection

```csharp
services.AddCommandQuery();
```

## Query By ID

```csharp
// sample user claims
var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));

var query = new EntityIdentifierQuery<int, ProductReadModel>(principal, 123);

// Send the query to the mediator for execution
var result = await mediator.Send(query);
```

## Query By Filter

```csharp
var filter = new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" };
var sort = new EntitySort { Name = "Name", Direction = "asc" };

var query = new EntitySelectQuery<ProductReadModel>(principal, filter, sort);

// Send the query to the mediator for execution
var result = await mediator.Send(query);
```

## Update Command

```csharp
var id = 123; // The ID of the entity to update
var updateModel = new ProductUpdateModel
{
    Name = "Updated Product",
    Description = "Updated description of the product",
    Price = 29.99m
};

var command = new EntityUpdateCommand<int, ProductUpdateModel, ProductReadModel>(principal, id, updateModel);

// Send the command to the mediator for execution
var result = await mediator.Send(command);
```
