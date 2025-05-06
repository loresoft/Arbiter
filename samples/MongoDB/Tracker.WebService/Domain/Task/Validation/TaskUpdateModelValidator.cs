#pragma warning disable IDE0130 // Namespace does not match folder structure

using FluentValidation;

using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Domain.Validation;

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
