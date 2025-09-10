using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Validation;

namespace Tracker.Domain;

public static class ValidationRegistration
{
    public static void Register(this IServiceCollection services)
    {
        services.AddValidation();
    }
}

[ValidatableType]
public class Person
{
    [Required]
    public string Name { get; set; } = null!;
    public int Age { get; set; }
    public int Birth { get; set; }
}
