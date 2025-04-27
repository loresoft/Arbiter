namespace Tracker.Client.Stores.Abstracts;

public abstract class StateBase<TModel>
{
    public event Action? OnChange;

    public TModel? Model { get; protected set; }

    public virtual void Set(TModel? model)
    {
        Model = model;

        NotifyStateChanged();
    }

    public virtual void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}
