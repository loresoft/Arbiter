using System;
using FluentValidation;
using Tracker.Domain.Models;

namespace Tracker.Domain.Validation;

[RegisterSingleton<FluentValidation.IValidator<UserCreateModel>>]
public partial class UserCreateModelValidator
    : FluentValidation.AbstractValidator<UserCreateModel>
{
    public UserCreateModelValidator()
    {
        #region Generated Constructor
        RuleFor(p => p.DisplayName).NotEmpty();
        RuleFor(p => p.DisplayName).MaximumLength(256);
        RuleFor(p => p.EmailAddress).NotEmpty();
        RuleFor(p => p.EmailAddress).MaximumLength(256);
        #endregion
    }

}
