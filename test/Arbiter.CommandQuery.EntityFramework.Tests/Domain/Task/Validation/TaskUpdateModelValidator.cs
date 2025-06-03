using System;
using FluentValidation;
using Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Validation;

[RegisterSingleton<FluentValidation.IValidator<TaskUpdateModel>>]
public partial class TaskUpdateModelValidator
    : AbstractValidator<TaskUpdateModel>
{
    public TaskUpdateModelValidator()
    {
        #region Generated Constructor
        RuleFor(p => p.Title).NotEmpty();
        RuleFor(p => p.Title).MaximumLength(255);
        RuleFor(p => p.UpdatedBy).MaximumLength(100);
        #endregion
    }

}
