using Arbiter.CommandQuery.Definitions;

using Blazored.Modal.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Tracker.Client.Services;
using Tracker.Client.Stores.Abstracts;

namespace Tracker.Client.Components.Abstracts;

public abstract class StorePageBase<TStore, TReadModel, TUpdateModel> : PrincipalBase, IDisposable
    where TStore : StoreEditBase<TReadModel, TUpdateModel>
    where TReadModel : class, IHaveIdentifier<int>, new()
    where TUpdateModel : class, new()
{
    [CascadingParameter]
    public required IModalService Modal { get; set; }

    [Inject]
    public required TStore Store { get; set; }

    [Inject]
    public required NotificationService Notification { get; set; }

    [Inject]
    public required NavigationManager Navigation { get; set; }

    [Inject]
    public required IJSRuntime JavaScript { get; set; }


    [Parameter, EditorRequired]
    public required int Id { get; set; }


    protected bool IsCreate => EqualityComparer<int>.Default.Equals(Id, default);


    protected abstract string Redirect { get; }


    protected override async Task OnInitializedAsync()
    {
        Store.OnChange += HandleModelChange;

        try
        {
            if (IsCreate)
                Store.New();
            else
                await Store.Load(Id);


            if (Store.Model == null)
            {
                Navigation.NavigateTo(Redirect);
                return;
            }

            await LoadAdditionalData();
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

    protected void HandleModelChange()
    {
        InvokeAsync(StateHasChanged);
    }

    protected virtual Task LoadAdditionalData()
    {
        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
        Store.OnChange -= HandleModelChange;
        GC.SuppressFinalize(this);
    }
}
