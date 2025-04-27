using System;
using FluentValidation;
using Tracker.Domain.Models;

namespace Tracker.Domain.Validation;

[RegisterSingleton<FluentValidation.IValidator<TaskCreateModel>>]
public partial class TaskCreateModelValidator
    : FluentValidation.AbstractValidator<TaskCreateModel>
{
    public TaskCreateModelValidator()
    {
        #region Generated Constructor
        RuleFor(p => p.Title).NotEmpty();
        RuleFor(p => p.Title).MaximumLength(255);
        #endregion
    }

}
