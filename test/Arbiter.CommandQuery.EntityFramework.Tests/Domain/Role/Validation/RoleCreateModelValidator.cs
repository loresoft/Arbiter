using System;
using FluentValidation;
using Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Validation;

[RegisterSingleton<FluentValidation.IValidator<RoleCreateModel>>]
public partial class RoleCreateModelValidator
    : AbstractValidator<RoleCreateModel>
{
    public RoleCreateModelValidator()
    {
        #region Generated Constructor
        RuleFor(p => p.Name).NotEmpty();
        RuleFor(p => p.Name).MaximumLength(256);
        RuleFor(p => p.CreatedBy).MaximumLength(100);
        RuleFor(p => p.UpdatedBy).MaximumLength(100);
        #endregion
    }

}
