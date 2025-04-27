using System;
using FluentValidation;
using Tracker.Domain.Models;

namespace Tracker.Domain.Validation;

[RegisterSingleton<FluentValidation.IValidator<PriorityCreateModel>>]
public partial class PriorityCreateModelValidator
    : FluentValidation.AbstractValidator<PriorityCreateModel>
{
    public PriorityCreateModelValidator()
    {
        #region Generated Constructor
        RuleFor(p => p.Name).NotEmpty();
        RuleFor(p => p.Name).MaximumLength(100);
        RuleFor(p => p.Description).MaximumLength(255);
        #endregion
    }

}
