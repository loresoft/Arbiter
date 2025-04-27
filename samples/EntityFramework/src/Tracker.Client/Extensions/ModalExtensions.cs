using Blazored.Modal;
using Blazored.Modal.Services;

using Tracker.Client.Components;

namespace Tracker.Client.Extensions;

public static class ModalExtensions
{
    public static async Task<bool> ConfirmDelete(this IModalService modalService, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(modalService);

        message ??= "Are you sure you want to delete this?";

        var parameters = new ModalParameters
        {
            { nameof(ConfirmModal.Message), message }
        };

        var messageForm = modalService.Show<ConfirmModal>("Confirm Delete", parameters);
        var result = await messageForm.Result;

        return !result.Cancelled;
    }

    public static async Task<bool> ConfirmNavigation(this IModalService modalService, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(modalService);

        message ??= "Are you sure you want to navigate away? Changes you made will be lost.";

        var parameters = new ModalParameters
        {
            { nameof(ConfirmModal.Message), message },
            { nameof(ConfirmModal.ActionName), "Yes" },
            { nameof(ConfirmModal.CancelName), "No" },
        };

        var messageForm = modalService.Show<ConfirmModal>("Confirm Navigation", parameters);
        var result = await messageForm.Result;

        return !result.Cancelled;
    }
}
