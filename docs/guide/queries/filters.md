# Query Filters

Common query filter classes

## `EntityFilter` Class

Represents a filter for selecting entities based on specific criteria.

This class is typically used in queries to define filtering criteria for entities. Filters can be combined using logical operators such as "and" or "or" and can include nested filters for complex queries.

```c#
public class EntityFilter
```

### `EntityFilter` Examples

The following example demonstrates how to use the `EntityFilter` class as a basic filter:

```c#
var filter = new EntityFilter
{
    Name = "Status",
    Operator = "eq",
    Value = "Active"
};
```

The following example demonstrates how to use the EntityFilter class as group filter:

```c#
var filter = new EntityFilter
{
    Logic = "and",
    Filters = new List<EntityFilter>
    {
        new EntityFilter { Name = "Priority", Operator = "gt", Value = 1 },
        new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" }
    }
};
```

## `EntitySort` Class

Represents a sort expression for an entity, specifying the property to sort by and the sort direction.

This class is typically used in queries to define sorting criteria for entities. The sort direction can be ascending ("asc") or descending ("desc").

```c#
public class EntitySort
```

### `EntitySort` Examples

The following example demonstrates how to use the `EntitySort` class:

```c#
var sort = new EntitySort
{
    Name = "Name",
    Direction = "asc"
};

Console.WriteLine(sort.ToString()); // Output: "Name:asc"
```

## `EntitySelect` Class

Represents a query for selecting entities with optional filtering and sorting criteria.

This class is typically used to define the selection criteria for querying entities, including filters, sorting, and raw query expressions.

```c#
public class EntitySelect
```

### `EntitySelect` Examples

The following example demonstrates how to use the `EntitySelect` class:

```c#
var filter = new EntityFilter
{
    Name = "Status",
    Operator = "eq",
    Value = "Active"
};

var sort = new EntitySort
{
    Name = "Name",
    Direction = "asc"
};

var entitySelect = new EntitySelect(filter, sort);
```

## `EntityQuery` Class

Represents a query for selecting entities with filtering, sorting, and pagination capabilities.

This class is typically used to define the criteria for querying entities, including filters, sorting, and pagination options.

```c#
public class EntityQuery
```

### `EntityQuery` Examples

The following example demonstrates how to use the `EntityQuery` class:

```c#
var filter = new EntityFilter
{
    Name = "Status",
    Operator = "eq",
    Value = "Active"
};

var sort = new EntitySort
{
    Name = "Name",
    Direction = "asc"
};

var query = new EntityQuery(filter, sort, page: 1, pageSize: 20);
Console.WriteLine($"Page: {query.Page}, PageSize: {query.PageSize}");
```
