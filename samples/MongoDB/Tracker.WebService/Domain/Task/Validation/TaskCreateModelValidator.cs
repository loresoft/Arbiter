#pragma warning disable IDE0130 // Namespace does not match folder structure

using FluentValidation;

using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Domain.Validation;

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
