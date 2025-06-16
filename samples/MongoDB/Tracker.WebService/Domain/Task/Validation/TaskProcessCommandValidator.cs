#pragma warning disable IDE0130 // Namespace does not match folder structure

using FluentValidation;

using Tracker.WebService.Domain.Task.Commands;

namespace Tracker.WebService.Domain.Validation;

[RegisterSingleton<IValidator<TaskProcessCommand>>]
public class TaskProcessCommandValidator : AbstractValidator<TaskProcessCommand>
{
    public TaskProcessCommandValidator()
    {
        RuleFor(p => p.Action).NotEmpty().WithMessage("Action is required.");
        RuleFor(p => p.Principal).NotNull().WithMessage("Principal is required.");
    }
}
