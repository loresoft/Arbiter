using System.Security.Claims;

using Arbiter.CommandQuery.Commands;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// A base query type using the specified <see cref="ClaimsPrincipal"/>.
/// </summary>
public abstract record PrincipalQueryBase : PrincipalCommandBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrincipalQueryBase"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> this query is run for</param>
    protected PrincipalQueryBase(ClaimsPrincipal? principal) : base(principal)
    {
    }
}
