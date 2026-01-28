using System.Diagnostics.CodeAnalysis;
using System.Net;

using Arbiter.CommandQuery.Models;

namespace Arbiter.CommandQuery.Extensions;

/// <summary>
/// Extension methods for <see cref="Exception"/>.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Flattens an <see cref="AggregateException"/> into a single <see cref="Exception"/> instance.
    /// </summary>
    [return: NotNullIfNotNull(nameof(exception))]
    public static Exception? Flatten(this Exception? exception)
    {
        if (exception == null)
            return null;

        if (exception is not AggregateException aggregateException)
            return exception;

        // flatten aggregate exceptions with a single inner exception
        aggregateException = aggregateException.Flatten();

        return aggregateException.InnerExceptions?.Count == 1
            ? aggregateException.InnerExceptions[0]
            : aggregateException;
    }

    /// <summary>
    /// Converts an exception to a <see cref="ProblemDetails"/> object.
    /// </summary>
    /// <param name="exception">The exception to convert</param>
    /// <returns>A <see cref="ProblemDetails"/> instance based on the exception</returns>
    public static ProblemDetails ToProblemDetails(this Exception exception)
    {
        var problemDetails = new ProblemDetails();

        var workingException = Flatten(exception);

        switch (workingException)
        {
            case System.ComponentModel.DataAnnotations.ValidationException validationException:
                var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

                if (validationException.ValidationResult.ErrorMessage != null)
                {
                    foreach (var memberName in validationException.ValidationResult.MemberNames)
                        errors[memberName] = [validationException.ValidationResult.ErrorMessage];
                }

                problemDetails.Title = "One or more validation errors occurred.";
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Extensions.Add("errors", errors);
                break;

            case DomainException domainException:
                var reasonPhrase = GetReasonPhrase(domainException.StatusCode);
                if (reasonPhrase.IsNullOrWhiteSpace())
                    reasonPhrase = "Internal Server Error";

                problemDetails.Title = reasonPhrase;
                problemDetails.Status = domainException.StatusCode;

                if (domainException.Errors != null)
                    problemDetails.Extensions.Add("errors", domainException.Errors);

                break;
            default:
                problemDetails.Title = "Internal Server Error.";
                problemDetails.Status = 500;
                break;
        }

        if (workingException == null)
            return problemDetails;

        var baseException = workingException.GetBaseException();

        problemDetails.Detail = baseException.Message;
        problemDetails.Extensions.Add("message", workingException.Message);
        problemDetails.Extensions.Add("exception", workingException.ToString());

        return problemDetails;
    }

    /// <summary>
    /// Gets the HTTP reason phrase for the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The reason phrase for common status codes, or a generic "Status {code}" for others.</returns>
    private static string GetReasonPhrase(int statusCode)
    {
        return statusCode switch
        {
            (int)HttpStatusCode.BadRequest => "Bad Request",
            (int)HttpStatusCode.Unauthorized => "Unauthorized",
            (int)HttpStatusCode.Forbidden => "Forbidden",
            (int)HttpStatusCode.NotFound => "Not Found",
            (int)HttpStatusCode.InternalServerError => "Internal Server Error",
            _ => $"Status {statusCode}",
        };
    }
}
