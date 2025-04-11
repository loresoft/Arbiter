using System.Security.Claims;

using Arbiter.CommandQuery.Commands;

namespace Arbiter.CommandQuery.Queries;

public abstract record PrincipalQueryBase<TResponse> : PrincipalCommandBase<TResponse>
{
    protected PrincipalQueryBase(ClaimsPrincipal? principal) : base(principal)
    {
    }
}
