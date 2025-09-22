using Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

using FluentValidation;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Validation;

[RegisterSingleton<FluentValidation.IValidator<AuditCreateModel>>]
public partial class AuditCreateModelValidator
    : AbstractValidator<AuditCreateModel>
{
    public AuditCreateModelValidator()
    {
        #region Generated Constructor
        RuleFor(p => p.Content).NotEmpty();
        RuleFor(p => p.Username).NotEmpty();
        RuleFor(p => p.Username).MaximumLength(50);
        RuleFor(p => p.CreatedBy).MaximumLength(100);
        RuleFor(p => p.UpdatedBy).MaximumLength(100);
        #endregion
    }

}
