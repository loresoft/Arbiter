using FluentValidation;
using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

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
