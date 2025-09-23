using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

using FluentValidation;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Validation;

[RegisterSingleton<IValidator<TaskUpdateModel>>]
public partial class TaskUpdateModelValidator
    : AbstractValidator<TaskUpdateModel>
{
    public TaskUpdateModelValidator()
    {

        RuleFor(p => p.Title).NotEmpty();
        RuleFor(p => p.Title).MaximumLength(255);

    }

}
