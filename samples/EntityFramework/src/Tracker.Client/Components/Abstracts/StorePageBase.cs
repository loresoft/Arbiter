using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.State;

using Blazored.Modal.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Tracker.Client.Services;

namespace Tracker.Client.Components.Abstracts;

public abstract class StorePageBase<TReadModel, TUpdateModel> : PrincipalBase, IDisposable
    where TReadModel : class, IHaveIdentifier<int>, new()
    where TUpdateModel : class, new()
{
    [CascadingParameter]
    public required IModalService Modal { get; set; }

    [Inject]
    public required ModelStateEditor<int, TReadModel, TUpdateModel> Store { get; set; }

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
        Store.OnStateChanged += HandleModelChange;

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

    protected void HandleModelChange(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    protected virtual Task LoadAdditionalData()
    {
        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
        Store.OnStateChanged -= HandleModelChange;
        GC.SuppressFinalize(this);
    }
}
