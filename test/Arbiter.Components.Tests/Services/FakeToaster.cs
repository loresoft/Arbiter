using LoreSoft.Blazor.Controls;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;

namespace Arbiter.Components.Tests.Services;

/// <summary>
/// An <see cref="IToaster"/> that records the toasts it is asked to show instead of displaying them.
/// </summary>
internal sealed class FakeToaster : IToaster
{
    public List<ShownToast> Toasts { get; } = [];

    public int ClearCount { get; private set; }

    public event Action<ToastLevel, RenderFragment, Action<ToastSettings>?> OnShow = delegate { };

    public event Action<ToastLevel?>? OnClear;

    public void Show(ToastLevel level, RenderFragment message, Action<ToastSettings>? settings = null)
    {
        var toastSettings = new ToastSettings();
        settings?.Invoke(toastSettings);

        Toasts.Add(new ShownToast(level, ReadText(message), toastSettings));

        OnShow.Invoke(level, message, settings);
    }

    public void Clear(ToastLevel? toastLevel = null)
    {
        ClearCount++;

        OnClear?.Invoke(toastLevel);
    }

    private static string ReadText(RenderFragment fragment)
    {
        var builder = new RenderTreeBuilder();
        fragment(builder);

        var frames = builder.GetFrames();
        var text = new System.Text.StringBuilder();

        // BL0006: RenderTreeFrame is not a supported public API, but inspecting the frames is the only way to
        // read the text of a RenderFragment without hosting a full renderer in this test double.
#pragma warning disable BL0006
        for (var index = 0; index < frames.Count; index++)
        {
            var frame = frames.Array[index];

            if (frame.FrameType == RenderTreeFrameType.Text)
                text.Append(frame.TextContent);
            else if (frame.FrameType == RenderTreeFrameType.Markup)
                text.Append(frame.MarkupContent);
        }
#pragma warning restore BL0006

        return text.ToString();
    }

    internal sealed record ShownToast(ToastLevel Level, string Message, ToastSettings Settings);
}
