using System.Security.Claims;

using Arbiter.CommandQuery.Commands;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// A base query type using the specified <see cref="ClaimsPrincipal"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public abstract record PrincipalQueryBase<TResponse> : PrincipalCommandBase<TResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrincipalQueryBase{TResponse}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> this query is run for</param>
    protected PrincipalQueryBase(ClaimsPrincipal? principal) : base(principal)
    {
    }
}
