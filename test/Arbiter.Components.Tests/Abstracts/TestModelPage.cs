using Arbiter.Components.Abstracts;

namespace Arbiter.Components.Tests.Abstracts;

/// <summary>
/// A read model used to verify the model label and name derived from the type name.
/// </summary>
public record PurchaseOrderReadModel
{
    public string? Number { get; init; }
}

/// <summary>
/// A read model whose type name does not use one of the recognized model suffixes.
/// </summary>
public record Supplier;

/// <summary>
/// A concrete <see cref="ModelComponentBase{TReadModel}"/> exposing the protected members to the tests.
/// </summary>
public class TestModelPage : ModelComponentBase<PurchaseOrderReadModel>
{
    public string? DisplayName { get; set; }

    public bool Dirty { get; set; }

    protected override string? ModelDisplay => DisplayName;

    protected override bool IsDirty => Dirty;

    public string PublicModelLabel => ModelLabel;

    public string PublicModelName => ModelName;

    public bool PublicIsDisposed => IsDisposed;

    public CancellationToken PublicCancellationToken => CancellationToken;

    public string PublicPageTitle(string? suffix = null) => PageTitle(suffix);

    public void PublicObserve(Func<CancellationToken, Task>? operation = null, bool notifyError = true)
        => Observe(operation, notifyError);

    public void RaiseModelChange() => HandleModelChange(this, EventArgs.Empty);
}

/// <summary>
/// A concrete <see cref="ModelComponentBase{TReadModel}"/> for a model type without a recognized suffix.
/// </summary>
public class TestSupplierPage : ModelComponentBase<Supplier>
{
    public string PublicModelLabel => ModelLabel;

    public string PublicModelName => ModelName;
}
