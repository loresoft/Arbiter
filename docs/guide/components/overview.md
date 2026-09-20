---
title: Components Overview
description: Blazor building blocks for Arbiter applications, providing base components for list, view, and edit pages, notification and principal services, and navigation helpers
---

# Components Overview

The `Arbiter.Components` package provides reusable Blazor building blocks for Arbiter applications. It supplies base page components for the common list, view, and edit patterns, along with notification and principal services, base address resolution, and navigation helpers. The base components build on the `IDispatcher` abstraction from `Arbiter.Dispatcher.Client`, so pages send commands and queries without depending on a specific transport.

## Features

- Base page components for **view**, **edit**, **list**, **result**, and **nested list** patterns
- A two-model edit pattern that separates the server `TReadModel` snapshot from the editable `TUpdateModel`
- Built-in change tracking, busy state, cancellation, and disposal handling
- An `INotificationService` for short-lived, non-blocking toast messages
- A `PrincipalComponentBase` for authorization, claims, and role access
- Navigation, data grid, and string helper extensions

## Installation

```powershell
Install-Package Arbiter.Components
```

OR

```shell
dotnet add package Arbiter.Components
```

## Dependencies

`Arbiter.Components` targets `net8.0`, `net9.0`, and `net10.0` and depends on:

| Package                                         | Purpose                                                            |
| :---------------------------------------------- | :----------------------------------------------------------------- |
| `Arbiter.Dispatcher.Client`                     | `IDispatcher`, `DispatcherDataService`, and model state management |
| `LoreSoft.Blazor.Controls`                      | `DataGrid`, `DataList`, `Toaster`, and `ModalService`              |
| `Microsoft.AspNetCore.Components.Authorization` | Authentication state and authorization services                    |
| `Microsoft.AspNetCore.Components.Web`           | Blazor web components                                              |

## Service registration

Call `AddArbiterComponents` when configuring the client application. It registers the notification service, base address resolver, and a principal-aware data service, and calls `AddBlazorControls()` for `Toaster` support.

| Service                  | Implementation         | Lifetime  |
| :----------------------- | :--------------------- | :-------- |
| `INotificationService`   | `NotificationService`  | Scoped    |
| `IBaseAddressResolver`   | `BaseAddressResolver`  | Scoped    |
| `IDispatcherDataService` | `PrincipalDataService` | Transient |

The following registration is used in `samples/EntityFramework/src/Tracker.Client/Services/ServiceRegistration.cs`:

```csharp
// Tracker.Client/Services/ServiceRegistration.cs
public static void Register(IServiceCollection services, ISet<string> tags)
{
    // component libraries
    services.AddBlazorControls();

    // page component services
    services.AddArbiterComponents();

    // ... dispatcher registration
}
```

### Configuring notification options

The `configureNotifications` overload configures how notifications are displayed. During development, `ShowExceptionDetails` can be enabled to surface full exception messages in error toasts:

```csharp
services
    .AddBlazorControls()
    .AddArbiterComponents(options => options.ShowExceptionDetails = true);
```

See [Component Services](services.md) for the full list of `NotificationServiceOptions` settings.

### Authentication state

`PrincipalDataService` and `PrincipalComponentBase` resolve the current user from the Blazor authentication state. Register the authorization and cascading authentication state services in `Program.cs`:

```csharp
// Program.cs (WebAssembly client)
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services
    .AddAuthorizationCore()
    .AddCascadingAuthenticationState()
    .AddAuthenticationStateDeserialization();
```

## Base component hierarchy

The base components share behavior through a small hierarchy. Choose the base class that matches the page pattern.

| Base component                                                   | Use for                                                                                    |
| :--------------------------------------------------------------- | :----------------------------------------------------------------------------------------- |
| `PrincipalComponentBase`                                         | Components that need the current user, claims, roles, or authorization policies            |
| `ViewPageBase<TKey, TReadModel>`                                 | Displaying a single read model loaded by its identifier — see [View Page](view.md)         |
| `EditPageBase<TKey, TReadModel, TUpdateModel>`                   | Creating or editing a single model — see [Edit Page](edit.md)                              |
| `ListPageBase<TKey, TReadModel>`                                 | Paged lists loaded through a `DataGrid` — see [List Pages](list.md)                        |
| `ResultPageBase<TReadModel>`                                     | A full result set loaded at once for search, charts, or export — see [List Pages](list.md) |
| `NestedListPageBase<TKey, TParentModel, TReadModel, TListModel>` | A parent model with a related, paged child list — see [List Pages](list.md)                |

`ModelComponentBase<TReadModel>` and `DataPageBase<TReadModel>` provide the shared behavior (display labels, page titles, cancellation, disposal, and data component subscriptions) that the concrete page bases inherit; they are not typically used directly.

## Redirect reasons

Page components report why they navigate away from the current model using the `RedirectReason` enum. Override `GetRedirectLocation(RedirectReason reason, TKey id)` to control the destination.

| Value      | Description                            |
| :--------- | :------------------------------------- |
| `NotFound` | The requested model could not be found |
| `Created`  | A new model was created                |
| `Canceled` | The user canceled the operation        |
| `Deleted`  | The model was deleted                  |

## Next steps

- [View Page](view.md) — display a single read model
- [Edit Page](edit.md) — create and edit models with the two-model pattern
- [List Pages](list.md) — paged lists, result sets, and nested lists
- [Component Services](services.md) — notifications, principal access, and base address resolution
- [Component Extensions](extensions.md) — navigation, data grid, and string helpers
