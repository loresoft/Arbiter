using Arbiter.CommandQuery.Definitions;

namespace Arbiter.CommandQuery.MongoDB.Tests.Adapters;

[RegisterSingleton(Duplicate = DuplicateStrategy.Replace)]
internal class FluentValidationAdapter<T>(FluentValidation.IValidator<T>? validator = null) : IValidator<T>
{
    public async ValueTask<Models.ValidationResult> Validate(T instance, CancellationToken cancellationToken = default)
    {
        if (validator is null)
            return Models.ValidationResult.Success;

        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (result.IsValid)
            return Models.ValidationResult.Success;

        return new Models.ValidationResult
        {
            Errors = result.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).ToArray(),
                    StringComparer.Ordinal)
        };
    }
}
