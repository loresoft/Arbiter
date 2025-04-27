using System;
using FluentValidation;
using Tracker.Domain.Models;

namespace Tracker.Domain.Validation;

[RegisterSingleton<FluentValidation.IValidator<TaskUpdateModel>>]
public partial class TaskUpdateModelValidator
    : FluentValidation.AbstractValidator<TaskUpdateModel>
{
    public TaskUpdateModelValidator()
    {
        #region Generated Constructor
        RuleFor(p => p.Title).NotEmpty();
        RuleFor(p => p.Title).MaximumLength(255);
        #endregion
    }

}
