using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

using FluentValidation;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Validation;

[RegisterSingleton<IValidator<AuditCreateModel>>]
public partial class AuditCreateModelValidator
    : AbstractValidator<AuditCreateModel>
{
    public AuditCreateModelValidator()
    {

        RuleFor(p => p.Content).NotEmpty();
        RuleFor(p => p.Username).NotEmpty();
        RuleFor(p => p.Username).MaximumLength(50);

    }

}
