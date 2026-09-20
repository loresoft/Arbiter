---
title: Component Extensions
description: Navigation, data grid, and string helper extensions provided by Arbiter.Components
---

# Component Extensions

`Arbiter.Components` includes a small set of extension methods that support the base page components. They can also be used directly from custom components.

## Navigation extensions

`NavigationManagerExtensions` adds safe navigation helpers to `NavigationManager`.

### NavigateLocalOnly

`NavigateLocalOnly` navigates only to in-application URLs, guarding against open-redirect attacks. Use it when the destination comes from an untrusted source such as a `returnUrl` query parameter:

```csharp
[Inject]
public required NavigationManager Navigation { get; set; }

// only navigates when the target is a local, in-app URL
Navigation.NavigateLocalOnly(returnUrl ?? "/");
```

### NotFound

`NotFound` signals that the requested resource was not found. It provides compatibility for .NET 8 and .NET 9, where the behavior is not built in; on .NET 10 and later the framework provides it natively.

```csharp
Navigation.NotFound();
```

The view and list base components use this internally when a model cannot be loaded. See [View Page](view.md#handling-not-found) for how not-found redirects are handled at the page level.

## Data grid extensions

`DataGridExtensions` converts a `LoreSoft.Blazor.Controls` grid request into an Arbiter `EntityQuery`, carrying paging, sorting, and filtering. The `ListPageBase` and related base components call `ToQuery()` internally inside `LoadData`, so most pages do not need to call it directly. When implementing a custom `LoadData`, use it to build the query:

```csharp
protected override async ValueTask<DataResult<TModel>> LoadData(DataRequest request)
{
    var query = request.ToQuery();
    query.Filter = CombineFilter(query.Filter);

    var results = await DataService.Page<TModel>(query);
    return results.ToResult();
}
```

## String extensions

`StringExtensions` provides `ToMarkupString`, which renders a string as HTML markup rather than encoded text. Use it only with trusted content, since it bypasses HTML encoding:

```razor
@* renders trusted HTML without encoding *@
@someTrustedHtml.ToMarkupString()
```

> [!WARNING]
> `ToMarkupString` does not encode its input. Never pass user-supplied or otherwise untrusted content to it, as doing so can introduce cross-site scripting (XSS) vulnerabilities.

## Next steps

- [Components Overview](overview.md) — registration and configuration
- [Component Services](services.md) — notifications and principal access
