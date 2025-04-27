using System;
using FluentValidation;
using Tracker.Domain.Models;

namespace Tracker.Domain.Validation;

[RegisterSingleton<FluentValidation.IValidator<TenantCreateModel>>]
public partial class TenantCreateModelValidator
    : FluentValidation.AbstractValidator<TenantCreateModel>
{
    public TenantCreateModelValidator()
    {
        #region Generated Constructor
        RuleFor(p => p.Name).NotEmpty();
        RuleFor(p => p.Name).MaximumLength(100);
        RuleFor(p => p.Description).MaximumLength(255);
        #endregion
    }

}
