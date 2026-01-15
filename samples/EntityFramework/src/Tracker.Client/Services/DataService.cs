using System.Security.Claims;

using Arbiter.Dispatcher;
using Arbiter.Dispatcher.Client;

using Microsoft.AspNetCore.Components.Authorization;

namespace Tracker.Client.Services;

[RegisterTransient]
public class DataService : DispatcherDataService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public DataService(IDispatcher dispatcher, AuthenticationStateProvider authenticationStateProvider) : base(dispatcher)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    public override async ValueTask<ClaimsPrincipal?> GetUser(CancellationToken cancellationToken = default)
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return state?.User;
    }
}
