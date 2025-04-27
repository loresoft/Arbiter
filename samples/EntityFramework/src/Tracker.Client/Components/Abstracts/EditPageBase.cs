using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;

using Microsoft.AspNetCore.Components.Forms;

using Tracker.Client.Extensions;
using Tracker.Client.Stores.Abstracts;


namespace Tracker.Client.Components.Abstracts;

public abstract class EditPageBase<TStore, TReadModel, TUpdateModel> : StorePageBase<TStore, TReadModel, TUpdateModel>
    where TStore : StoreEditBase<TReadModel, TUpdateModel>
    where TReadModel : class, IHaveIdentifier<int>, new()
    where TUpdateModel : class, new()
{
    protected EditContext? EditContext { get; set; }

    protected abstract string ModelTypeName { get; }

    protected abstract string? ModelInstanceName { get; }


    protected string PageTitle()
    {
        if (ModelInstanceName.HasValue() && Store.IsDirty)
            return $"{ModelTypeName} - {ModelInstanceName} *";

        if (ModelInstanceName.HasValue())
            return $"{ModelTypeName} - {ModelInstanceName}";

        return ModelTypeName;
    }

    protected string EditLabel() => IsCreate ? "Create" : "Edit";

    protected string EditTitle() => $"{ModelTypeName} {EditLabel()}";

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (Store.Model == null)
        {
            Navigation.NavigateTo(Redirect);
            return;
        }

        EditContext = new EditContext(Store.Model!);
        EditContext.OnFieldChanged += HandleFormChange;
    }


    protected virtual async Task HandleSave()
    {
        try
        {
            var originalId = Store.Original?.Id ?? default;

            await Store.Save();

            Notification.ShowSuccess($"{ModelTypeName} '{ModelInstanceName}' saved successfully");

            var updatedId = Store.Original?.Id ?? default;
            if (updatedId == originalId)
                return;

            Navigation.NavigateTo(Redirect);
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

    protected virtual async Task HandleDelete()
    {
        try
        {
            if (IsCreate || Store.Model == null)
                return;

            if (!await Modal.ConfirmDelete($"Are you sure you want to delete {ModelTypeName} '{ModelInstanceName}'?"))
                return;

            await Store.Delete();

            Notification.ShowSuccess($"{ModelTypeName} '{ModelInstanceName}' deleted successfully");
            Navigation.NavigateTo(Redirect);
        }
        catch (Exception ex)
        {
            Notification.ShowError(ex);
        }
    }


    protected void HandleFormChange(object? sender, FieldChangedEventArgs args)
    {
        Store.NotifyStateChanged();
    }

    public override void Dispose()
    {
        base.Dispose();

        if (EditContext != null)
            EditContext.OnFieldChanged -= HandleFormChange;

        GC.SuppressFinalize(this);
    }
}
