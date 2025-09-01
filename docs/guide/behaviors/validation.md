# Validation Behaviors

Pipeline behaviors that validate commands and entity models before processing to ensure data integrity and business rule compliance. These behaviors work in conjunction with entity commands to provide automatic validation capabilities.

## Overview

The Arbiter framework provides two validation behavior implementations that work with entity commands and custom commands to ensure data integrity and business rule compliance:

- **ValidateEntityModelCommandBehavior** - Validates entity models using FluentValidation or custom validators
- **ValidateCommandBehavior** - Validates entire command requests including business rules and authorization

Both validation behaviors integrate seamlessly with `EntityCreateCommand`, `EntityUpdateCommand`, and `EntityDeleteCommand` to provide automatic validation without requiring additional configuration in command handlers.

**Key Features:**

- **Automatic Detection**: Commands automatically detect and apply configured validators
- **Early Termination**: Stops command processing if validation fails to prevent invalid operations
- **FluentValidation Integration**: Works seamlessly with FluentValidation validators and custom validation logic
- **Detailed Error Messages**: Provides comprehensive validation error details for better user experience
- **Command Integration**: Works seamlessly with `EntityCreateCommand`, `EntityUpdateCommand`, and `EntityDeleteCommand`

## ValidateEntityModelCommandBehavior

The `ValidateEntityModelCommandBehavior<TEntityModel, TResponse>` behavior automatically validates entity models before command processing using configurable validation rules. This ensures data integrity and prevents invalid entities from being persisted.

### Entity Model Validation Characteristics

- **Pre-command validation** of entity models before handler execution
- **FluentValidation integration** with automatic validator discovery
- **Detailed error reporting** with property-specific validation messages
- **Graceful handling** when no validators are configured

### Entity Model Validation Use Cases

- **Data Integrity**: Ensure required fields are present and properly formatted
- **Business Rules**: Validate complex business constraints on entity properties
- **Input Sanitization**: Validate and sanitize user input before persistence
- **Compliance**: Enforce regulatory or business compliance rules on entity data

### Configuration

#### Using FluentValidation

```csharp
// Define validation rules
public class UserCreateModelValidator : AbstractValidator<UserCreateModel>
{
    public UserCreateModelValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);
            
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);
            
        RuleFor(x => x.Age)
            .GreaterThan(0)
            .LessThan(150);
    }
}

// Register validator
services.AddTransient<IValidator<UserCreateModel>, UserCreateModelValidator>();
```

#### Custom Validation Logic

```csharp
public class CustomUserValidator : IValidator<UserCreateModel>
{
    public ValidationResult Validate(UserCreateModel instance)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrEmpty(instance.Email))
            result.Errors.Add(new ValidationFailure("Email", "Email is required"));
            
        if (!instance.Email.Contains("@"))
            result.Errors.Add(new ValidationFailure("Email", "Email must be valid"));
            
        return result;
    }
    
    // Additional IValidator<T> implementation...
}
```

### Example Entity and Model

```csharp
// Command model with validation attributes
public class UserCreateModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? PhoneNumber { get; set; }
}

// Entity
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? PhoneNumber { get; set; }
}
```

## ValidateCommandBehavior

The `ValidateCommandBehavior<TRequest, TResponse>` behavior validates entire command requests, including command-specific properties and business rules that extend beyond entity model validation.

### Command Validation Characteristics

- **Whole-command validation** including context, permissions, and business rules
- **Business logic enforcement** that spans multiple properties or entities
- **Authorization integration** with user permissions and role-based validation
- **Contextual validation** based on tenant, user state, or system conditions

### Command Validation Use Cases

- **Authorization Rules**: Validate user permissions for specific operations
- **Business Constraints**: Enforce complex business rules that span multiple properties
- **Cross-Entity Validation**: Validate relationships between entities
- **Contextual Rules**: Apply validation based on tenant, user role, or system state

### Example Implementation

```csharp
// Command with complex validation requirements
public class TransferFundsCommand : IRequest<TransferResult>
{
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public ClaimsPrincipal Principal { get; set; }
}

// Command validator
public class TransferFundsCommandValidator : AbstractValidator<TransferFundsCommand>
{
    public TransferFundsCommandValidator(IAccountService accountService)
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Transfer amount must be positive");
            
        RuleFor(x => x.FromAccountId)
            .NotEqual(x => x.ToAccountId)
            .WithMessage("Cannot transfer to the same account");
            
        RuleFor(x => x)
            .MustAsync(async (command, cancellation) => 
            {
                var account = await accountService.GetAccountAsync(command.FromAccountId);
                return account?.Balance >= command.Amount;
            })
            .WithMessage("Insufficient funds for transfer");
    }
}
```

## Entity Commands with Validation

The validation behaviors integrate seamlessly with entity commands to provide automatic validation without requiring modifications to command handlers.

### EntityCreateCommand Validation

The `EntityCreateCommand` automatically applies both entity model validation and command validation:

```csharp
// Create command with automatic validation
var createCommand = new EntityCreateCommand<UserCreateModel, UserReadModel>(
    userCreateModel, 
    principal);

// Validation occurs automatically:
// 1. ValidateEntityModelCommandBehavior validates userCreateModel
// 2. ValidateCommandBehavior validates the entire command (if configured)
// 3. If validation passes, handler executes
var result = await mediator.Send(createCommand);
```

### EntityUpdateCommand Validation

Update commands validate both the updated model and command context:

```csharp
// Update command with validation
var updateCommand = new EntityUpdateCommand<int, UserUpdateModel, UserReadModel>(
    userId, 
    userUpdateModel, 
    principal);

// Automatic validation ensures data integrity before update
var result = await mediator.Send(updateCommand);
```

## Validation Error Handling

### DomainException Structure

When validation fails, behaviors throw a `DomainException` with structured error information:

```csharp
try
{
    var result = await mediator.Send(createUserCommand);
}
catch (DomainException ex)
{
    // ex.StatusCode typically 400 for validation errors
    // ex.Message contains validation summary
    // ex.Data may contain detailed validation errors
    
    foreach (var error in ex.ValidationErrors)
    {
        Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
    }
}
```

### Validation Error Response

```json
{
    "statusCode": 400,
    "message": "Validation failed",
    "errors": [
        {
            "propertyName": "Email",
            "errorMessage": "Email address is required",
            "attemptedValue": null
        },
        {
            "propertyName": "Age", 
            "errorMessage": "Age must be between 1 and 150",
            "attemptedValue": -5
        }
    ]
}
```

## Service Registration

### Automatic Registration with Entity Commands

Validation behaviors are automatically registered when using entity command registration:

```csharp
// Automatically registers ValidateEntityModelCommandBehavior if validators are found
services.AddEntityCommands<int, UserReadModel, UserCreateModel, UserUpdateModel>();
```

### Manual Registration

```csharp
// Entity model validation
services.AddTransient<IPipelineBehavior<EntityCreateCommand<UserCreateModel, UserReadModel>, UserReadModel>, 
    ValidateEntityModelCommandBehavior<UserCreateModel, UserReadModel>>();

// Command validation  
services.AddTransient<IPipelineBehavior<TransferFundsCommand, TransferResult>, 
    ValidateCommandBehavior<TransferFundsCommand, TransferResult>>();

// Register validators
services.AddTransient<IValidator<UserCreateModel>, UserCreateModelValidator>();
services.AddTransient<IValidator<TransferFundsCommand>, TransferFundsCommandValidator>();
```

### Conditional Registration

```csharp
// Register validation only in development/testing
if (builder.Environment.IsDevelopment())
{
    services.AddTransient<IValidator<UserCreateModel>, StrictUserCreateModelValidator>();
}
else
{
    services.AddTransient<IValidator<UserCreateModel>, BasicUserCreateModelValidator>();
}
```

## Best Practices

### Validation Strategy

1. **Layer Separation**: Use entity model validation for data integrity, command validation for business rules
2. **Performance**: Keep validation lightweight; offload expensive checks to handlers when possible
3. **User Experience**: Provide clear, actionable error messages
4. **Localization**: Consider internationalization for validation messages

### Validator Design

1. **Single Responsibility**: Keep validators focused on specific concerns
2. **Reusability**: Create base validators for common patterns
3. **Async Operations**: Use async validation sparingly and only for essential checks
4. **Dependencies**: Keep validator dependencies minimal and well-defined

### Error Handling

1. **Consistent Format**: Use standardized error response formats
2. **Security**: Avoid exposing sensitive information in validation messages
3. **Logging**: Log validation failures for monitoring and debugging
4. **Recovery**: Provide guidance for resolving validation errors

### Testing

1. **Unit Tests**: Test validators independently with various input scenarios
2. **Integration Tests**: Test validation behavior within the command pipeline
3. **Edge Cases**: Test boundary conditions and error scenarios
4. **Performance Tests**: Ensure validation doesn't introduce significant latency
