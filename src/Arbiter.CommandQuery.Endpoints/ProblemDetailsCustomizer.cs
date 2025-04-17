using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Service to customize the ProblemDetails response for exceptions.
/// </summary>
public static class ProblemDetailsCustomizer
{
    /// <summary>
    /// Configures the <see cref="ProblemDetailsOptions"/> for customizing the ProblemDetails response.
    /// </summary>
    /// <param name="options">The problem details options</param>
    public static void Configure(ProblemDetailsOptions options)
    {
        options.CustomizeProblemDetails = CustomizeProblemDetails;
    }

    /// <summary>
    /// Customizes the ProblemDetails response based on the exception type.
    /// </summary>
    /// <param name="context">The problem details context</param>
    public static void CustomizeProblemDetails(ProblemDetailsContext context)
    {
        var exception = GetException(context);

        switch (exception)
        {
            //case FluentValidation.ValidationException fluentException:
            //{

            //    var errors = fluentException.Errors
            //        .GroupBy(x => x.PropertyName, StringComparer.Ordinal)
            //        .ToDictionary(
            //            keySelector: g => g.Key,
            //            elementSelector: g => g.Select(x => x.ErrorMessage).ToArray(),
            //            comparer: StringComparer.Ordinal);

            //    AddValidationErrors(context, errors);

            //    context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            //    context.ProblemDetails.Status = StatusCodes.Status400BadRequest;
            //    break;
            //}
            case System.ComponentModel.DataAnnotations.ValidationException validationException:
            {
                var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

                if (validationException.ValidationResult.ErrorMessage != null)
                    foreach (var memberName in validationException.ValidationResult.MemberNames)
                        errors[memberName] = [validationException.ValidationResult.ErrorMessage];

                AddValidationErrors(context, errors);

                context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.ProblemDetails.Status = StatusCodes.Status400BadRequest;
                break;
            }
            case Arbiter.CommandQuery.DomainException domainException:
            {
                context.HttpContext.Response.StatusCode = domainException.StatusCode;
                context.ProblemDetails.Status = domainException.StatusCode;

                break;
            }
        }

        context.ProblemDetails.Detail = exception?.Message;
        context.ProblemDetails.Extensions.Add("trace-id", context.HttpContext.TraceIdentifier);

        var env = context.HttpContext.RequestServices.GetService<IWebHostEnvironment>();
        if (exception is not null && env != null && env.IsDevelopment())
        {
            context.ProblemDetails.Extensions.Add("exception", exception?.ToString());
        }
    }

    private static Exception? GetException(ProblemDetailsContext context)
    {
#if NET8_0_OR_GREATER
        var exception = context.Exception;
#else
        var feature = context.HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = feature?.Error;
#endif
        return exception;
    }

    private static void AddValidationErrors(ProblemDetailsContext context, IDictionary<string, string[]> errors)
    {
#if NET8_0_OR_GREATER
        var validationProblem = new HttpValidationProblemDetails(errors);
        context.ProblemDetails = validationProblem;
#else
        context.ProblemDetails.Extensions.Add("errors", errors);
#endif
    }

    /// <summary>
    /// Converts an exception to a <see cref="ProblemDetails"/> object.
    /// </summary>
    /// <param name="exception">The exception to convert</param>
    /// <returns>A <see cref="ProblemDetails"/> instance based on the exception</returns>
    public static ProblemDetails ToProblemDetails(this Exception exception)
    {
        var problemDetails = new ProblemDetails();
        switch (exception)
        {
            //case FluentValidation.ValidationException fluentException:
            //{
            //    var errors = fluentException.Errors
            //        .GroupBy(x => x.PropertyName, StringComparer.Ordinal)
            //        .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray(), StringComparer.Ordinal);

            //    problemDetails.Title = "One or more validation errors occurred.";
            //    problemDetails.Status = StatusCodes.Status400BadRequest;
            //    problemDetails.Extensions.Add("errors", errors);
            //    break;
            //}
            case System.ComponentModel.DataAnnotations.ValidationException validationException:
            {
                var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

                if (validationException.ValidationResult.ErrorMessage != null)
                {
                    foreach (var memberName in validationException.ValidationResult.MemberNames)
                        errors[memberName] = [validationException.ValidationResult.ErrorMessage];
                }

                problemDetails.Title = "One or more validation errors occurred.";
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Extensions.Add("errors", errors);
                break;
            }
            case Arbiter.CommandQuery.DomainException domainException:
            {
                problemDetails.Title = "Internal Server Error.";
                problemDetails.Status = domainException.StatusCode;
                break;
            }
            default:
            {
                problemDetails.Title = "Internal Server Error.";
                problemDetails.Status = 500;
                break;
            }
        }

        if (exception != null)
        {
            problemDetails.Detail = exception.Message;
            problemDetails.Extensions.Add("exception", exception.ToString());
        }

        return problemDetails;
    }
}
