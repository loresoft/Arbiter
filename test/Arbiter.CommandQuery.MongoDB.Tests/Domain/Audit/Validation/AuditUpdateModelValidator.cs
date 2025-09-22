using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

using FluentValidation;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Validation;

[RegisterSingleton<IValidator<AuditUpdateModel>>]
public partial class AuditUpdateModelValidator
    : AbstractValidator<AuditUpdateModel>
{
    public AuditUpdateModelValidator()
    {

        RuleFor(p => p.Content).NotEmpty();
        RuleFor(p => p.Username).NotEmpty();
        RuleFor(p => p.Username).MaximumLength(50);

    }
}
