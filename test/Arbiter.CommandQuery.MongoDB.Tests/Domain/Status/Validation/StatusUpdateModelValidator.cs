using FluentValidation;
using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Validation;

[RegisterSingleton<IValidator<StatusUpdateModel>>]
public partial class StatusUpdateModelValidator
    : AbstractValidator<StatusUpdateModel>
{
    public StatusUpdateModelValidator()
    {

        RuleFor(p => p.Name).NotEmpty();
        RuleFor(p => p.Name).MaximumLength(100);
        RuleFor(p => p.Description).MaximumLength(255);

    }

}
