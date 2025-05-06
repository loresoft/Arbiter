#pragma warning disable IDE0130 // Namespace does not match folder structure

using FluentValidation;

using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Domain.Validation;

[RegisterSingleton<IValidator<PriorityCreateModel>>]
public partial class PriorityCreateModelValidator
    : AbstractValidator<PriorityCreateModel>
{
    public PriorityCreateModelValidator()
    {

        RuleFor(p => p.Name).NotEmpty();
        RuleFor(p => p.Name).MaximumLength(100);
        RuleFor(p => p.Description).MaximumLength(255);

    }

}
