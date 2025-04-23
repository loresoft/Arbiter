using FluentValidation;
using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Validation;

[RegisterSingleton<IValidator<RoleUpdateModel>>]
public partial class RoleUpdateModelValidator
    : AbstractValidator<RoleUpdateModel>
{
    public RoleUpdateModelValidator()
    {

        RuleFor(p => p.Name).NotEmpty();
        RuleFor(p => p.Name).MaximumLength(256);

    }

}
