using Arbiter.CommandQuery.Definitions;
using Arbiter.Dispatcher.State;

using Rocks;

namespace Arbiter.CommandQuery.Tests.State;

public class ModelStateEditorTests
{
    // Test model classes implementing IHaveIdentifier
    private class TestReadModel : IHaveIdentifier<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Description);
        }
    }

    private class TestUpdateModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Description);
        }
    }

    // Simple test mapper implementation
    private class TestMapper : IMapper
    {
        public TDestination? Map<TSource, TDestination>(TSource? source)
        {
            if (source is null)
                return default;

            // Handle specific mappings for our test types
            if (source is TestUpdateModel updateModel && typeof(TDestination) == typeof(TestReadModel))
            {
                var mappedReadModel = new TestReadModel
                {
                    Id = 0, // Default for new items
                    Name = updateModel.Name,
                    Description = updateModel.Description
                };
                return (TDestination)(object)mappedReadModel;
            }

            if (source is TestReadModel readModel && typeof(TDestination) == typeof(TestUpdateModel))
            {
                var updateModelResult = new TestUpdateModel
                {
                    Name = readModel.Name,
                    Description = readModel.Description
                };
                return (TDestination)(object)updateModelResult;
            }

            throw new NotSupportedException($"Mapping from {typeof(TSource)} to {typeof(TDestination)} is not supported");
        }

        public void Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            throw new NotImplementedException("Update mapping not used in these tests");
        }

        public IQueryable<TDestination> ProjectTo<TSource, TDestination>(IQueryable<TSource> source)
        {
            throw new NotImplementedException("ProjectTo not used in these tests");
        }
    }

    [Test]
    public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Arrange
        var mockService = new MockDataService();
        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        // Act
        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Assert
        manager.DataService.Should().BeSameAs(dataService);
        manager.Mapper.Should().BeSameAs(mapper);
        manager.Model.Should().BeNull();
        manager.Original.Should().BeNull();
        manager.IsBusy.Should().BeFalse();
        manager.EditHash.Should().Be(0);
        manager.IsDirty.Should().BeTrue();
        manager.IsClean.Should().BeFalse();
    }

    [Test]
    public void Constructor_WithNullDataService_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mapper = new TestMapper();

        // Act & Assert
        var action = () => new ModelStateEditor<int, TestReadModel, TestUpdateModel>(null!, mapper);

        action.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("*dataService*");
    }

    [Test]
    public void Constructor_WithNullMapper_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockService = new MockDataService();
        var dataService = mockService.Instance();

        // Act & Assert
        var action = () => new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, null!);

        action.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("*mapper*");
    }

    [Test]
    public void Set_WithValidModel_ShouldSetModelAndUpdateProperties()
    {
        // Arrange
        var mockService = new MockDataService();
        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var updateModel = new TestUpdateModel { Name = "Test Update", Description = "Test Description" };
        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Act
        manager.Set(updateModel);

        // Assert
        manager.Model.Should().BeSameAs(updateModel);
        manager.Original.Should().NotBeNull();
        manager.Original!.Name.Should().Be("Test Update");
        manager.Original.Description.Should().Be("Test Description");
        manager.EditHash.Should().Be(updateModel.GetHashCode());
        manager.IsDirty.Should().BeFalse(); // Just set, so not dirty
        manager.IsClean.Should().BeTrue();
    }

    [Test]
    public void Set_WithNull_ShouldClearAllProperties()
    {
        // Arrange
        var mockService = new MockDataService();
        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Act
        manager.Set(null);

        // Assert
        manager.Model.Should().BeNull();
        manager.Original.Should().BeNull();
        manager.EditHash.Should().Be(0);
        manager.IsDirty.Should().BeTrue();
        manager.IsClean.Should().BeFalse();
    }

    [Test]
    public void Set_ShouldTriggerStateChangedEvent()
    {
        // Arrange
        var mockService = new MockDataService();
        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var updateModel = new TestUpdateModel { Name = "Test", Description = "Test" };
        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        var eventTriggered = false;
        manager.OnStateChanged += (sender, args) =>
        {
            eventTriggered = true;
            sender.Should().Be(manager);
            args.Should().Be(EventArgs.Empty);
        };

        // Act
        manager.Set(updateModel);

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [Test]
    public void Clear_ShouldResetAllProperties()
    {
        // Arrange
        var mockService = new MockDataService();
        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var updateModel = new TestUpdateModel { Name = "Test", Description = "Test" };
        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);
        manager.Set(updateModel); // Set initial state

        // Act
        manager.Clear();

        // Assert
        manager.Model.Should().BeNull();
        manager.Original.Should().BeNull();
        manager.EditHash.Should().Be(0);
        manager.IsDirty.Should().BeTrue();
        manager.IsClean.Should().BeFalse();
    }

    [Test]
    public void New_ShouldCreateNewReadModelAndConvertToUpdateModel()
    {
        // Arrange
        var mockService = new MockDataService();
        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Act
        manager.New();

        // Assert
        manager.Model.Should().NotBeNull();
        manager.Original.Should().NotBeNull();
        manager.Original!.Id.Should().Be(0); // New model has default ID
        manager.EditHash.Should().Be(manager.Model!.GetHashCode());
        manager.IsDirty.Should().BeFalse();
        manager.IsClean.Should().BeTrue();
    }

    [Test]
    public async Task Load_WithValidId_ShouldLoadModelAndSetProperties()
    {
        // Arrange
        var testReadModel = new TestReadModel { Id = 123, Name = "Test Model", Description = "Test Description" };

        var mockService = new MockDataService();
        mockService.Setups
            .Get<int, TestReadModel>(123)
            .ReturnValue(ValueTask.FromResult<TestReadModel?>(testReadModel));

        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Act
        await manager.Load(123);

        // Assert
        manager.Model.Should().NotBeNull();
        manager.Model!.Name.Should().Be("Test Model");
        manager.Model.Description.Should().Be("Test Description");
        manager.Original.Should().BeSameAs(testReadModel);
        manager.EditHash.Should().Be(manager.Model.GetHashCode());
        manager.IsBusy.Should().BeFalse();
        manager.IsDirty.Should().BeFalse();
        manager.IsClean.Should().BeTrue();

        mockService.Verify();
    }

    [Test]
    public async Task Load_WithNonExistentId_ShouldSetModelsToNull()
    {
        // Arrange
        var mockService = new MockDataService();
        mockService.Setups
            .Get<int, TestReadModel>(999)
            .ReturnValue(ValueTask.FromResult<TestReadModel?>(null));

        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Act
        await manager.Load(999);

        // Assert
        manager.Model.Should().BeNull();
        manager.Original.Should().BeNull();
        manager.EditHash.Should().Be(0);
        manager.IsBusy.Should().BeFalse();
        manager.IsDirty.Should().BeTrue();
        manager.IsClean.Should().BeFalse();

        mockService.Verify();
    }

    [Test]
    public async Task Load_ShouldTriggerStateChangedEvents()
    {
        // Arrange
        var testReadModel = new TestReadModel { Id = 123, Name = "Test" };

        var mockService = new MockDataService();
        mockService.Setups
            .Get<int, TestReadModel>(123)
            .ReturnValue(ValueTask.FromResult<TestReadModel?>(testReadModel));

        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);
        var eventCount = 0;
        var busyStates = new List<bool>();

        manager.OnStateChanged += (sender, args) =>
        {
            eventCount++;
            busyStates.Add(manager.IsBusy);
        };

        // Act
        await manager.Load(123);

        // Assert
        eventCount.Should().Be(2); // Start and end of load
        busyStates.Should().HaveCount(2);
        busyStates[0].Should().BeTrue();  // IsBusy = true at start
        busyStates[1].Should().BeFalse(); // IsBusy = false at end

        mockService.Verify();
    }

    [Test]
    public async Task Load_WhenDataServiceThrows_ShouldResetIsBusyAndPropagateException()
    {
        // Arrange
        var mockService = new MockDataService();
        mockService.Setups
            .Get<int, TestReadModel>(123)
            .Callback((_, _, _) => throw new InvalidOperationException("Test exception"));

        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Act & Assert
        var action = async () => await manager.Load(123);

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");

        manager.IsBusy.Should().BeFalse();
        manager.Model.Should().BeNull();
        manager.Original.Should().BeNull();

        mockService.Verify();
    }

    [Test]
    public async Task Load_WithForceParameterFalse_SameIdAlreadyLoaded_ShouldNotCallDataService()
    {
        // Arrange
        var testReadModel = new TestReadModel { Id = 123, Name = "Test Model" };

        var mockService = new MockDataService();
        mockService.Setups
            .Get<int, TestReadModel>(123)
            .ReturnValue(ValueTask.FromResult<TestReadModel?>(testReadModel))
            .ExpectedCallCount(1);

        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Act - Load first time
        await manager.Load(123);

        // Act - Load second time with same ID (should be skipped)
        await manager.Load(123, force: false);

        // Assert
        manager.Model.Should().NotBeNull();
        manager.Original.Should().BeSameAs(testReadModel);

        mockService.Verify();
    }

    [Test]
    public async Task Load_WithForceParameterTrue_SameIdAlreadyLoaded_ShouldCallDataServiceAgain()
    {
        // Arrange
        var mockService = new MockDataService();

        mockService.Setups
            .Get<int, TestReadModel>(123)
            .Callback((_, _, _) =>
                ValueTask.FromResult<TestReadModel?>(new TestReadModel
                {
                    Id = 123,
                    Name = $"Test {DateTime.Now.Ticks}"
                })
            )
            .ExpectedCallCount(2);

        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Act - Load first time
        await manager.Load(123);
        var firstModel = manager.Model;

        // Act - Force load second time with same ID
        await manager.Load(123, force: true);
        var secondModel = manager.Model;

        // Assert
        firstModel.Should().NotBeNull();
        secondModel.Should().NotBeNull();
        firstModel.Should().NotBeSameAs(secondModel);

        mockService.Verify();
    }

    [Test]
    public async Task Save_WithValidModel_ShouldSaveAndUpdateState()
    {
        // Arrange
        var updateModel = new TestUpdateModel { Name = "Updated Name", Description = "Updated Description" };
        var savedReadModel = new TestReadModel { Id = 123, Name = "Updated Name", Description = "Updated Description" };

        var mockService = new MockDataService();
        mockService.Setups
            .Save<int, TestUpdateModel, TestReadModel>(0, updateModel) // Default key for new item
            .ReturnValue(ValueTask.FromResult<TestReadModel?>(savedReadModel));

        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);
        manager.Set(updateModel); // Set initial state

        // Act
        await manager.Save();

        // Assert
        manager.Model.Should().NotBeNull();
        manager.Model!.Name.Should().Be("Updated Name");
        manager.Model.Description.Should().Be("Updated Description");
        manager.Original.Should().BeSameAs(savedReadModel);
        manager.EditHash.Should().Be(manager.Model.GetHashCode());
        manager.IsBusy.Should().BeFalse();
        manager.IsDirty.Should().BeFalse();
        manager.IsClean.Should().BeTrue();

        mockService.Verify();
    }

    [Test]
    public async Task Save_WithExistingModel_ShouldUseOriginalId()
    {
        // Arrange
        var originalReadModel = new TestReadModel { Id = 456, Name = "Original", Description = "Original" };
        var updateModel = new TestUpdateModel { Name = "Updated Name", Description = "Updated Description" };
        var savedReadModel = new TestReadModel { Id = 456, Name = "Updated Name", Description = "Updated Description" };

        var mockService = new MockDataService();
        mockService.Setups
            .Get<int, TestReadModel>(456)
            .ReturnValue(ValueTask.FromResult<TestReadModel?>(originalReadModel));
        mockService.Setups
            .Save<int, TestUpdateModel, TestReadModel>(456, Arg.Any<TestUpdateModel>()) // Should use existing ID
            .ReturnValue(ValueTask.FromResult<TestReadModel?>(savedReadModel));

        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Load existing model first
        await manager.Load(456);

        // Modify the model
        manager.Model!.Name = "Updated Name";
        manager.Model.Description = "Updated Description";

        // Act
        await manager.Save();

        // Assert
        manager.Original!.Id.Should().Be(456);
        mockService.Verify();
    }

    [Test]
    public async Task Save_WithNullModel_ShouldReturnWithoutSaving()
    {
        // Arrange
        var mockService = new MockDataService();
        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Act
        await manager.Save();

        // Assert
        manager.Model.Should().BeNull();
        manager.Original.Should().BeNull();
        manager.IsBusy.Should().BeFalse();

        // Verify no service calls were made
        mockService.Verify();
    }

    [Test]
    public async Task Save_ShouldTriggerStateChangedEvents()
    {
        // Arrange
        var updateModel = new TestUpdateModel { Name = "Test", Description = "Test" };
        var savedReadModel = new TestReadModel { Id = 123, Name = "Test", Description = "Test" };

        var mockService = new MockDataService();
        mockService.Setups
            .Save<int, TestUpdateModel, TestReadModel>(0, updateModel)
            .ReturnValue(ValueTask.FromResult<TestReadModel?>(savedReadModel));

        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);
        manager.Set(updateModel);

        var eventCount = 0;
        var busyStates = new List<bool>();

        manager.OnStateChanged += (sender, args) =>
        {
            eventCount++;
            busyStates.Add(manager.IsBusy);
        };

        // Act
        await manager.Save();

        // Assert
        eventCount.Should().BeGreaterThanOrEqualTo(2); // At least start and end of save
        busyStates.Should().Contain(true);  // Should have been busy at some point
        busyStates.Last().Should().BeFalse(); // Should not be busy at the end

        mockService.Verify();
    }

    [Test]
    public async Task Save_WhenDataServiceThrows_ShouldResetIsBusyAndPropagateException()
    {
        // Arrange
        var updateModel = new TestUpdateModel { Name = "Test", Description = "Test" };

        var mockService = new MockDataService();
        mockService.Setups
            .Save<int, TestUpdateModel, TestReadModel>(0, updateModel)
            .Callback((_, _, _) => throw new InvalidOperationException("Save failed"));

        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);
        manager.Set(updateModel);

        // Act & Assert
        var action = async () => await manager.Save();

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Save failed");

        manager.IsBusy.Should().BeFalse();

        mockService.Verify();
    }

    [Test]
    public async Task Delete_WithValidModel_ShouldDeleteAndClearState()
    {
        // Arrange
        var readModel = new TestReadModel { Id = 123, Name = "Test", Description = "Test" };

        var mockService = new MockDataService();
        mockService.Setups
            .Get<int, TestReadModel>(123)
            .ReturnValue(ValueTask.FromResult<TestReadModel?>(readModel));
        mockService.Setups
            .Delete<int, TestReadModel>(123)
            .ReturnValue(ValueTask.FromResult<TestReadModel?>(null));

        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Load model first
        await manager.Load(123);

        // Act
        await manager.Delete();

        // Assert
        manager.Model.Should().BeNull();
        manager.Original.Should().BeNull();
        manager.EditHash.Should().Be(0);
        manager.IsBusy.Should().BeFalse();
        manager.IsDirty.Should().BeTrue();
        manager.IsClean.Should().BeFalse();

        mockService.Verify();
    }

    [Test]
    public async Task Delete_WithNullModel_ShouldReturnWithoutDeleting()
    {
        // Arrange
        var mockService = new MockDataService();
        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Act
        await manager.Delete();

        // Assert
        manager.Model.Should().BeNull();
        manager.Original.Should().BeNull();
        manager.IsBusy.Should().BeFalse();

        // Verify no service calls were made
        mockService.Verify();
    }

    [Test]
    public async Task Delete_ShouldTriggerStateChangedEvents()
    {
        // Arrange
        var readModel = new TestReadModel { Id = 123, Name = "Test", Description = "Test" };

        var mockService = new MockDataService();
        mockService.Setups
            .Get<int, TestReadModel>(123)
            .ReturnValue(ValueTask.FromResult<TestReadModel?>(readModel));
        mockService.Setups
            .Delete<int, TestReadModel>(123)
            .ReturnValue(ValueTask.FromResult<TestReadModel?>(null));

        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Load model first
        await manager.Load(123);

        var eventCount = 0;
        var busyStates = new List<bool>();

        manager.OnStateChanged += (sender, args) =>
        {
            eventCount++;
            busyStates.Add(manager.IsBusy);
        };

        // Act
        await manager.Delete();

        // Assert
        eventCount.Should().BeGreaterThanOrEqualTo(2); // At least start and end of delete
        busyStates.Should().Contain(true);  // Should have been busy at some point
        busyStates.Last().Should().BeFalse(); // Should not be busy at the end

        mockService.Verify();
    }

    [Test]
    public async Task Delete_WhenDataServiceThrows_ShouldResetIsBusyAndPropagateException()
    {
        // Arrange
        var readModel = new TestReadModel { Id = 123, Name = "Test", Description = "Test" };

        var mockService = new MockDataService();
        mockService.Setups
            .Get<int, TestReadModel>(123)
            .ReturnValue(ValueTask.FromResult<TestReadModel?>(readModel));
        mockService.Setups
            .Delete<int, TestReadModel>(123)
            .Callback((_, _) => throw new InvalidOperationException("Delete failed"));

        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Load model first
        await manager.Load(123);

        // Act & Assert
        var action = async () => await manager.Delete();

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Delete failed");

        manager.IsBusy.Should().BeFalse();

        mockService.Verify();
    }

    [Test]
    public void IsDirty_WhenModelHashChanges_ShouldReturnTrue()
    {
        // Arrange
        var mockService = new MockDataService();
        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var updateModel = new TestUpdateModel { Name = "Original", Description = "Original" };
        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);
        manager.Set(updateModel);

        manager.IsDirty.Should().BeFalse(); // Initially clean

        // Act - Modify the model
        updateModel.Name = "Modified";

        // Assert
        manager.IsDirty.Should().BeTrue();
        manager.IsClean.Should().BeFalse();
    }

    [Test]
    public void IsClean_WhenModelHashMatches_ShouldReturnTrue()
    {
        // Arrange
        var mockService = new MockDataService();
        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var updateModel = new TestUpdateModel { Name = "Test", Description = "Test" };
        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Act
        manager.Set(updateModel);

        // Assert
        manager.IsClean.Should().BeTrue();
        manager.IsDirty.Should().BeFalse();
    }

    [Test]
    public void IsDirty_WithNullModel_ShouldReturnTrue()
    {
        // Arrange
        var mockService = new MockDataService();
        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        // Act & Assert
        manager.IsDirty.Should().BeTrue();
        manager.IsClean.Should().BeFalse();
    }

    [Test]
    public async Task Save_WithNewModel_ShouldUseDefaultKeyFromOriginal()
    {
        // Arrange
        var updateModel = new TestUpdateModel { Name = "New Item", Description = "New Description" };
        var savedReadModel = new TestReadModel { Id = 456, Name = "New Item", Description = "New Description" };
        var mappedUpdateModel = new TestUpdateModel { Name = "New Item", Description = "New Description" };

        var mockService = new MockDataService();
        mockService.Setups
            .Save<int, TestUpdateModel, TestReadModel>(0, Arg.Any<TestUpdateModel>()) // Should use default key for new items
            .ReturnValue(ValueTask.FromResult<TestReadModel?>(savedReadModel));

        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);
        manager.New(); // Create new model
        manager.Model!.Name = "New Item";
        manager.Model!.Description = "New Description";

        // Act
        await manager.Save();

        // Assert
        manager.Original!.Id.Should().Be(456);
        mockService.Verify();
    }

    [Test]
    public async Task Load_StateChangedEventHandling_ShouldProvideCorrectSenderAndArgs()
    {
        // Arrange
        var testReadModel = new TestReadModel { Id = 123, Name = "Test" };

        var mockService = new MockDataService();
        mockService.Setups
            .Get<int, TestReadModel>(123)
            .ReturnValue(ValueTask.FromResult<TestReadModel?>(testReadModel));

        var dataService = mockService.Instance();
        var mapper = new TestMapper();

        var manager = new ModelStateEditor<int, TestReadModel, TestUpdateModel>(dataService, mapper);

        var eventSenders = new List<object?>();
        var eventArgs = new List<EventArgs?>();

        manager.OnStateChanged += (sender, args) =>
        {
            eventSenders.Add(sender);
            eventArgs.Add(args);
        };

        // Act
        await manager.Load(123);

        // Assert
        eventSenders.Should().HaveCount(2);
        eventSenders.Should().AllSatisfy(sender => sender.Should().Be(manager));

        eventArgs.Should().HaveCount(2);
        eventArgs.Should().AllSatisfy(args => args.Should().Be(EventArgs.Empty));

        mockService.Verify();
    }
}
