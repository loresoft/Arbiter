using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

using FluentValidation;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Validation;

[RegisterSingleton<IValidator<TaskCreateModel>>]
public partial class TaskCreateModelValidator
    : AbstractValidator<TaskCreateModel>
{
    public TaskCreateModelValidator()
    {

        RuleFor(p => p.Title).NotEmpty();
        RuleFor(p => p.Title).MaximumLength(255);

    }

}
