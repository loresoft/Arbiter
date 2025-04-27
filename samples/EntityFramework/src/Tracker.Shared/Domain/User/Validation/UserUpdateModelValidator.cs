using System;
using FluentValidation;
using Tracker.Domain.Models;

namespace Tracker.Domain.Validation;

[RegisterSingleton<FluentValidation.IValidator<UserUpdateModel>>]
public partial class UserUpdateModelValidator
    : FluentValidation.AbstractValidator<UserUpdateModel>
{
    public UserUpdateModelValidator()
    {
        #region Generated Constructor
        RuleFor(p => p.DisplayName).NotEmpty();
        RuleFor(p => p.DisplayName).MaximumLength(256);
        RuleFor(p => p.EmailAddress).NotEmpty();
        RuleFor(p => p.EmailAddress).MaximumLength(256);
        #endregion
    }

}
