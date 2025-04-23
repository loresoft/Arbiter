// Ignore Spelling: Validator

using System.ComponentModel.DataAnnotations;

namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// Defines a validator for a particular type.
/// </summary>
/// <typeparam name="T"> The type to validate.</typeparam>
public interface IValidator<in T>
{
    /// <summary>
    /// Validate the specified instance asynchronously
    /// </summary>
    /// <param name="instance">The instance to validate</param>
    /// <param name="cancellationToken"> The cancellation token</param>
    /// <returns>A ValidationResult object containing any validation errors.</returns>
    ValueTask<Models.ValidationResult> Validate(T instance, CancellationToken cancellationToken = default);
}
