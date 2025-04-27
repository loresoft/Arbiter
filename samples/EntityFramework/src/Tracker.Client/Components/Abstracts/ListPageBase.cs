using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;

using Blazored.Modal.Services;

using LoreSoft.Blazor.Controls;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Tracker.Client.Extensions;
using Tracker.Client.Services;

namespace Tracker.Client.Components.Abstracts;

public abstract class ListPageBase<TReadModel>() : ListPageBase<TReadModel, TReadModel>
    where TReadModel : class, IHaveIdentifier<int>
{ }

public abstract class ListPageBase<TListModel, TReadModel> : PrincipalBase
    where TListModel : class, IHaveIdentifier<int>
    where TReadModel : class, IHaveIdentifier<int>
{
    protected ListPageBase()
    {
        SearchText = new DebounceValue<string>(HandleSearch);
    }

    [CascadingParameter]
    public required IModalService Modal { get; set; }

    [Inject]
    public required DataService DataService { get; set; }

    [Inject]
    public required NotificationService Notification { get; set; }

    [Inject]
    public required NavigationManager Navigation { get; set; }

    [Inject]
    public required IJSRuntime JavaScript { get; set; }


    protected DataGrid<TListModel>? DataGrid { get; set; }

    protected DebounceValue<string> SearchText { get; }

    protected virtual string? ModelTypeName { get; }


    protected virtual async ValueTask<DataResult<TListModel>> LoadData(DataRequest request)
    {
        try
        {
            var query = request.ToQuery();
            query.Filter = RewriteFilter(query.Filter);

            var results = await DataService.Page<TListModel>(query);

            await LoadAdditionalData();

            if (results == null)
                return new DataResult<TListModel>(0, []);

            return results.ToResult();
        }
        catch (Exception ex)
        {
            Notification.ShowError(ex);
            return new DataResult<TListModel>(0, []);
        }
        finally
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    protected virtual EntityFilter? RewriteFilter(EntityFilter? originalFilter) => originalFilter;

    protected virtual Task LoadAdditionalData() => Task.CompletedTask;


    protected async Task HandleDelete(TListModel model)
    {
        try
        {
            if (!await Modal.ConfirmDelete())
                return;

            await DataService.Delete<int, TReadModel>(model.Id);

            Notification.ShowSuccess($"Item deleted successfully");

            if (DataGrid != null)
                await DataGrid.RefreshAsync();
        }
        catch (Exception ex)
        {
            Notification.ShowError(ex);
        }
        finally
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    protected void HandleSearch(string? searchText)
    {
        if (DataGrid == null)
            return;

        InvokeAsync(async () => await DataGrid.QuickSearch(searchText));
    }

    protected async Task HandelRefresh()
    {
        if (DataGrid == null)
            return;

        await DataGrid.RefreshAsync();
    }

    protected void ToggleFilter()
    {
        if (DataGrid == null)
            return;

        DataGrid.ShowFilter();
    }
}
