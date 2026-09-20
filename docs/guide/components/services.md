---
title: Component Services
description: Notification, principal, and base address services provided by Arbiter.Components
---

# Component Services

`Arbiter.Components` registers a small set of services through `AddArbiterComponents`. This page covers the notification service, the principal component base and data service, and the base address resolver.

## Notification service

`INotificationService` displays short-lived, non-blocking notification messages. Messages are queued immediately and shown as toasts through `IToaster`; warnings and errors are also logged. Inject `INotificationService` into a component and call the appropriate method.

| Method                            | Description                               |
| :-------------------------------- | :---------------------------------------- |
| `ShowInformation(message)`        | Displays an informational message         |
| `ShowSuccess(message)`            | Displays a success message                |
| `ShowWarning(message)`            | Displays a warning message                |
| `ShowWarning(exception, message)` | Logs the exception and displays a warning |
| `ShowError(message)`              | Displays an error message                 |
| `ShowError(exception, message)`   | Logs the exception and displays an error  |
| `Clear()`                         | Dismisses all displayed notifications     |

A common pattern surfaces exceptions from a `try`/`catch` block. This example is adapted from `samples/EntityFramework/src/Tracker.Client/Components/EntitySelect.razor`:

```csharp
[Inject]
public required INotificationService Notification { get; set; }

protected async Task<IReadOnlyList<TModel>> LoadData()
{
    try
    {
        var query = new EntityQuery { Filter = Filter };
        query.AddSort(Sort ?? TModel.SortField());

        var result = await DataService.Page<TModel>(query, CacheTime);
        return result?.Data ?? [];
    }
    catch (Exception ex)
    {
        Notification.ShowError(ex);
        return [];
    }
    finally
    {
        await InvokeAsync(StateHasChanged);
    }
}
```

`ShowError` also accepts a plain string for validation-style messages:

```csharp
if (SecurityKeyId == 0)
{
    Notification.ShowError("Please select a security key.");
    return;
}
```

### Notification options

`NotificationServiceOptions` configures how notifications are displayed. Set them through the `AddArbiterComponents` overload — see [Components Overview](overview.md#configuring-notification-options).

| Option                 | Default      | Description                                                                                  |
| :--------------------- | :----------- | :------------------------------------------------------------------------------------------- |
| `ShowExceptionDetails` | `false`      | When `true`, includes full exception messages in error toasts. Intended for development only |
| `ErrorMessage`         | —            | The default message shown for errors when details are hidden                                 |
| `WarningMessage`       | —            | The default message shown for warnings                                                       |
| `WarningTimeout`       | `15` seconds | How long a warning notification is displayed                                                 |
| `ErrorTimeout`         | `30` seconds | How long an error notification is displayed                                                  |

## Principal component base

`PrincipalComponentBase` is the base component for pages that need the current user, claims, roles, or authorization policies. It reads from the cascading `AuthenticationState`, so the authentication and authorization services must be registered — see [Components Overview](overview.md#authentication-state).

The concrete page components (`ViewPageBase`, `EditPageBase`, and the list bases) already derive from `PrincipalComponentBase`, so their pages have access to the current user without additional setup. Inherit from it directly for custom pages that are not one of the standard patterns:

```razor
@page "/logging"

@inherits PrincipalComponentBase

@code {
    [Inject]
    public required INotificationService Notification { get; set; }

    [Inject]
    public required IDispatcher Dispatcher { get; set; }

    protected virtual async ValueTask<DataResult<LogRecord>> LoadData(DataRequest request)
    {
        try
        {
            var command = new LogRecordQuery
            {
                PageSize = request.PageSize,
                ContinuationToken = request.ContinuationToken
            };

            var results = await Dispatcher.Send(command);
            return new DataResult<LogRecord>(results?.Data ?? []);
        }
        catch (Exception ex)
        {
            Notification.ShowError(ex);
            return new DataResult<LogRecord>([]);
        }
    }
}
```

Custom base components can also extend `PrincipalComponentBase` to share behavior across a set of pages while retaining access to the current principal.

## Principal data service

`PrincipalDataService` extends `DispatcherDataService` and resolves the current user from the Blazor authentication state, preferring the cascaded authentication state task and falling back to the authentication state provider. `AddArbiterComponents` registers it as the `IDispatcherDataService` implementation, so commands and queries sent through the data service carry the current principal. No page-level code is required to use it.

## Base address resolver

`BaseAddressResolver` implements `IBaseAddressResolver` and resolves the Blazor application base address, preferring the `NavigationManager` base URI and falling back to configuration. The configuration key is `BaseAddress`. `AddArbiterComponents` registers it automatically; it is used internally when a component or service needs the application base address.

## Next steps

- [Component Extensions](extensions.md) — navigation, data grid, and string helpers
- [Components Overview](overview.md) — registration and configuration
