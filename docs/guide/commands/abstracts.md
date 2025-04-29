# Command Abstracts

Abstract base classes for commands

## `PrincipalCommandBase` Class

Represents a base command type that uses a specified `ClaimsPrincipal` to execute operations.

```c#
public abstract record PrincipalCommandBase<TResponse>(ClaimsPrincipal? principal)
```

### `PrincipalCommandBase` Type Parameters

`TResponse`

The type of the response returned by the command.

### `PrincipalCommandBase` Constructor Parameters

`principal ClaimsPrincipal`

The `ClaimsPrincipal` representing the user executing the command.

## `EntityIdentifierCommand` Class

Represents a base command for operations that require an identifier.

### `EntityIdentifierCommand` Type Parameters

`TKey`

The type of the key used to identify the entity.

`TResponse`

The type of the response returned by the command.

### `EntityIdentifierCommand` Constructor Parameters

`principal ClaimsPrincipal`

The ClaimsPrincipal representing the user executing the command.

`id TKey`

The identifier of the entity for this command.

## `EntityIdentifiersCommand` Class

Represents a base command for operations that require a list of identifiers.

### `EntityIdentifiersCommand` Type Parameters

`TKey`

The type of the key used to identify the entities.

`TResponse`

The type of the response returned by the command.

### `EntityIdentifiersCommand` Constructor Parameters

`principal ClaimsPrincipal`

The ClaimsPrincipal representing the user executing the command.

`ids IReadOnlyCollection<TKey>`

The collection of identifiers for this command.

## `EntityModelCommand` Class

Represents a base command that uses a view model to perform an operation.

### `EntityModelCommand` Type Parameters

`TEntityModel`

The type of the model used as input for the command.

`TReadModel`

The type of the read model returned as the result of the command.

### `EntityModelCommand` Constructor Parameters

`principal ClaimsPrincipal`

The ClaimsPrincipal representing the user executing the command.

`model TEntityModel`

The model containing the data for the operation.
