namespace Tracker.Client.Stores.Abstracts;

public abstract class StoreBase<TModel> : StateBase<TModel>
    where TModel : class, new()
{
    private readonly string _modelType = typeof(TModel).Name;

    protected StoreBase(ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger(GetType());
    }

    public ILogger Logger { get; }

    public override void Set(TModel? model)
    {
        base.Set(model);
        Logger.LogDebug("Store model '{ModelType}' changed.", _modelType);
    }

    public virtual void Clear()
    {
        base.Set(default);
        Logger.LogDebug("Store model '{ModelType}' cleared.", _modelType);
    }

    public virtual void New()
    {
        base.Set(new TModel());
        Logger.LogDebug("Store model '{ModelType}' created.", _modelType);
    }
}
