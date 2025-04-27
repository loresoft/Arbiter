using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Dispatcher;

using Tracker.Client.Services;

namespace Tracker.Client.Stores.Abstracts;

public abstract class StoreEditBase<TReadModel, TUpdateModel> : StoreBase<TUpdateModel>
    where TReadModel : class, IHaveIdentifier<int>, new()
    where TUpdateModel : class, new()
{
    protected StoreEditBase(ILoggerFactory loggerFactory, DataService dataService, IMapper mapper)
        : base(loggerFactory)
    {
        DataService = dataService;
        Mapper = mapper;
    }

    public DataService DataService { get; }

    public IMapper Mapper { get; }

    public IDispatcher Dispatcher => DataService.Dispatcher;

    public TReadModel? Original { get; protected set; }

    public int EditHash { get; protected set; }

    public bool IsBusy { get; protected set; }

    public bool IsDirty => Model?.GetHashCode() != EditHash;

    public bool IsClean => Model?.GetHashCode() == EditHash;


    public override void Set(TUpdateModel? model)
    {
        if (model == null)
        {
            Model = null;
            Original = null;
            EditHash = 0;
        }
        else
        {
            Model = model;

            // convert update to read model
            Original = Mapper.Map<TUpdateModel, TReadModel>(model);

            // save hash to track changes
            EditHash = Model.GetHashCode();
        }

        Logger.LogDebug("Store model '{modelType}' changed.", typeof(TUpdateModel).Name);
        NotifyStateChanged();
    }

    public override void Clear()
    {
        SetModel(null);

        Logger.LogDebug("Store model '{modelType}' cleared.", typeof(TUpdateModel).Name);
        NotifyStateChanged();
    }

    public override void New()
    {
        SetModel(new TReadModel());

        Logger.LogDebug("Store model '{modelType}' created.", typeof(TUpdateModel).Name);
        NotifyStateChanged();
    }


    public async Task Load(int id, bool force = false)
    {
        // don't load if already loaded
        if (!force && Original != null && Original.Id == id)
            return;

        try
        {
            IsBusy = true;

            // load read modal
            var readModel = await DataService.Get<int, TReadModel>(id);

            SetModel(readModel);
        }
        finally
        {
            IsBusy = false;
            NotifyStateChanged();
        }
    }

    public async Task Save()
    {
        try
        {
            IsBusy = true;

            // get key from original
            var key = Original != null ? Original.Id : 0;

            var readModel = await DataService.Save<int, TUpdateModel, TReadModel>(key, Model!);

            SetModel(readModel);
        }
        finally
        {
            IsBusy = false;
            NotifyStateChanged();
        }
    }

    public async Task Delete()
    {
        if (Model == null || Original == null)
            return;

        try
        {
            IsBusy = true;

            // get key from original
            await DataService.Delete<int, TReadModel>(Original.Id);

            SetModel(null);
        }
        finally
        {
            IsBusy = false;
            NotifyStateChanged();
        }
    }


    protected void SetModel(TReadModel? model)
    {
        if (model == null)
        {
            Original = null;
            Model = null;
            EditHash = 0;
        }
        else
        {
            Original = model;

            // convert read to update model
            Model = Mapper.Map<TReadModel, TUpdateModel>(model);

            // save hash to track changes
            EditHash = Model.GetHashCode();
        }
    }
}
