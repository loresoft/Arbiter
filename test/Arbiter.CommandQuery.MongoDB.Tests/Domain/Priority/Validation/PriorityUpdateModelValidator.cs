using FluentValidation;
using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Validation;

[RegisterSingleton<IValidator<PriorityUpdateModel>>]
public partial class PriorityUpdateModelValidator
    : AbstractValidator<PriorityUpdateModel>
{
    public PriorityUpdateModelValidator()
    {

        RuleFor(p => p.Name).NotEmpty();
        RuleFor(p => p.Name).MaximumLength(100);
        RuleFor(p => p.Description).MaximumLength(255);

    }

}
