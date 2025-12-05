using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.State;

namespace Arbiter.CommandQuery.Tests.State;



public class ModelStateLoaderTests
{
    // Test model class implementing IHaveIdentifier
    private class TestModel : IHaveIdentifier<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    [Test]
    public void Constructor_WithValidDataService_ShouldInitializeCorrectly()
    {
        // Arrange
        var mockService = new MockDataService();
        var dataService = mockService.Instance();

        // Act
        var manager = new ModelStateLoader<int, TestModel>(dataService);

        // Assert
        manager.DataService.Should().BeSameAs(dataService);
        manager.Model.Should().BeNull();
        manager.IsBusy.Should().BeFalse();
    }

    [Test]
    public void Constructor_WithNullDataService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new ModelStateLoader<int, TestModel>(null!);

        action.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("*dataService*");
    }

    [Test]
    public void IsBusy_InitialState_ShouldBeFalse()
    {
        // Arrange
        var mockService = new MockDataService();
        var dataService = mockService.Instance();

        var manager = new ModelStateLoader<int, TestModel>(dataService);

        // Act & Assert
        manager.IsBusy.Should().BeFalse();
    }

    [Test]
    public async Task Load_WithValidId_ShouldLoadModelAndSetProperties()
    {
        // Arrange
        var testModel = new TestModel { Id = 123, Name = "Test Model", Description = "Test Description" };

        var mockService = new MockDataService();

        mockService.Setups
            .Get<int, TestModel>(123)
            .ReturnValue(ValueTask.FromResult<TestModel?>(testModel));

        var dataService = mockService.Instance();

        var manager = new ModelStateLoader<int, TestModel>(dataService);

        // Act
        await manager.Load(123);

        // Assert
        manager.Model.Should().BeSameAs(testModel);
        manager.Model!.Id.Should().Be(123);
        manager.Model.Name.Should().Be("Test Model");
        manager.Model.Description.Should().Be("Test Description");
        manager.IsBusy.Should().BeFalse();
    }

    [Test]
    public async Task Load_WithNonExistentId_ShouldSetModelToNull()
    {
        // Arrange
        var mockService = new MockDataService();
        mockService.Setups
            .Get<int, TestModel>(999)
            .ReturnValue(ValueTask.FromResult<TestModel?>(null));

        var dataService = mockService.Instance();
        var manager = new ModelStateLoader<int, TestModel>(dataService);

        // Act
        await manager.Load(999);

        // Assert
        manager.Model.Should().BeNull();
        manager.IsBusy.Should().BeFalse();
    }

    [Test]
    public async Task Load_ShouldTriggerStateChangedEvents()
    {
        // Arrange
        var testModel = new TestModel { Id = 123, Name = "Test" };
        var mockService = new MockDataService();

        mockService.Setups
            .Get<int, TestModel>(123)
            .ReturnValue(ValueTask.FromResult<TestModel?>(testModel));

        var dataService = mockService.Instance();

        var manager = new ModelStateLoader<int, TestModel>(dataService);
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
            .Get<int, TestModel>(123)
            .Callback((_, _, _) => throw new InvalidOperationException("Test exception"));

        var dataService = mockService.Instance();

        var manager = new ModelStateLoader<int, TestModel>(dataService);

        // Act & Assert
        var action = async () => await manager.Load(123);

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");

        manager.IsBusy.Should().BeFalse();
        manager.Model.Should().BeNull();

        mockService.Verify();
    }

    [Test]
    public async Task Load_WithForceParameterFalse_SameIdAlreadyLoaded_ShouldNotCallDataService()
    {
        // Arrange
        var testModel = new TestModel { Id = 123, Name = "Test Model" };

        var mockService = new MockDataService();
        mockService.Setups
            .Get<int, TestModel>(123)
            .ReturnValue(ValueTask.FromResult<TestModel?>(testModel))
            .ExpectedCallCount(1);

        var dataService = mockService.Instance();

        var manager = new ModelStateLoader<int, TestModel>(dataService);

        // Act - Load first time
        await manager.Load(123);

        // Act - Load second time with same ID (should be skipped)
        await manager.Load(123, force: false);

        // Assert
        manager.Model.Should().BeSameAs(testModel);

        mockService.Verify();
    }

    [Test]
    public async Task Load_WithForceParameterTrue_SameIdAlreadyLoaded_ShouldCallDataServiceAgain()
    {
        // Arrange
        var mockService = new MockDataService();
        mockService.Setups
            .Get<int, TestModel>(123)
            .Callback((_, _, _) =>
                ValueTask.FromResult<TestModel?>(new TestModel
                {
                    Id = 123,
                    Name = $"First {DateTime.Now.Ticks}"
                })
            )
            .ExpectedCallCount(2);

        var dataService = mockService.Instance();

        var manager = new ModelStateLoader<int, TestModel>(dataService);

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
        firstModel.Name.Should().NotBe(secondModel.Name);

        mockService.Verify();
    }

    [Test]
    public async Task Load_WithDifferentIds_ShouldAlwaysCallDataService()
    {
        // Arrange
        var mockService = new MockDataService();
        mockService.Setups
            .Get<int, TestModel>(123)
            .ReturnValue(ValueTask.FromResult<TestModel?>(new TestModel
            {
                Id = 123,
                Name = "Model 1"
            }))
            .ExpectedCallCount(1);

        mockService.Setups
            .Get<int, TestModel>(456)
            .ReturnValue(ValueTask.FromResult<TestModel?>(new TestModel
            {
                Id = 456,
                Name = "Model 2"
            }))
            .ExpectedCallCount(1);

        var dataService = mockService.Instance();

        var manager = new ModelStateLoader<int, TestModel>(dataService);

        // Act
        await manager.Load(123);
        var firstModel = manager.Model;

        await manager.Load(456);
        var secondModel = manager.Model;

        // Assert
        firstModel.Should().NotBeNull();
        secondModel.Should().NotBeNull();

        firstModel.Should().NotBeSameAs(secondModel);

        firstModel.Id.Should().Be(123);
        secondModel.Id.Should().Be(456);

        mockService.Verify();
    }

    [Test]
    public async Task Load_WithNullIdAfterValidId_ShouldSkipDuplicateLoad()
    {
        // Arrange
        var testModel = new TestModel { Id = 123, Name = "Test Model" };

        var mockService = new MockDataService();
        mockService.Setups
            .Get<int, TestModel>(123)
            .ReturnValue(ValueTask.FromResult<TestModel?>(testModel))
            .ExpectedCallCount(2);


        var dataService = mockService.Instance();

        var manager = new ModelStateLoader<int, TestModel>(dataService);

        // Load a valid model first
        await manager.Load(123);

        // Clear the model by setting to null
        manager.Set(null);

        // Act - Try to load with force=false after model is null
        await manager.Load(123, force: false);

        // Assert
        manager.Model.Should().BeSameAs(testModel);

        mockService.Verify();
    }

    [Test]
    public async Task Load_StateChangedEventHandling_ShouldProvideCorrectSenderAndArgs()
    {
        // Arrange
        var testModel = new TestModel { Id = 123, Name = "Test" };

        var mockService = new MockDataService();
        mockService.Setups
            .Get<int, TestModel>(123)
            .ReturnValue(ValueTask.FromResult<TestModel?>(testModel))
            .ExpectedCallCount(1);

        var dataService = mockService.Instance();

        var manager = new ModelStateLoader<int, TestModel>(dataService);

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
