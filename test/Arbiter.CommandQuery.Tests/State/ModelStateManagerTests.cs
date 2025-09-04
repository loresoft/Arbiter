using Arbiter.CommandQuery.State;

namespace Arbiter.CommandQuery.Tests.State;

public class ModelStateManagerTests
{
    // Test model class for testing
    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [Test]
    public void Model_WhenCreated_ShouldBeNull()
    {
        // Arrange & Act
        var manager = new ModelStateManager<TestModel>();

        // Assert
        manager.Model.Should().BeNull();
    }

    [Test]
    public void Set_WithValidModel_ShouldSetModel()
    {
        // Arrange
        var manager = new ModelStateManager<TestModel>();
        var model = new TestModel { Name = "Test", Value = 42 };

        // Act
        manager.Set(model);

        // Assert
        manager.Model.Should().BeSameAs(model);
        manager.Model!.Name.Should().Be("Test");
        manager.Model.Value.Should().Be(42);
    }

    [Test]
    public void Set_WithNull_ShouldSetModelToNull()
    {
        // Arrange
        var manager = new ModelStateManager<TestModel>();
        var model = new TestModel { Name = "Test", Value = 42 };
        manager.Set(model);

        // Act
        manager.Set(null);

        // Assert
        manager.Model.Should().BeNull();
    }

    [Test]
    public void Set_WhenCalled_ShouldTriggerOnStateChangedEvent()
    {
        // Arrange
        var manager = new ModelStateManager<TestModel>();
        var eventTriggered = false;
        EventArgs? capturedArgs = null;
        object? capturedSender = null;

        manager.OnStateChanged += (sender, args) =>
        {
            eventTriggered = true;
            capturedSender = sender;
            capturedArgs = args;
        };

        var model = new TestModel { Name = "Test" };

        // Act
        manager.Set(model);

        // Assert
        eventTriggered.Should().BeTrue();
        capturedSender.Should().BeSameAs(manager);
        capturedArgs.Should().Be(EventArgs.Empty);
    }

    [Test]
    public void Clear_WhenCalled_ShouldSetModelToNull()
    {
        // Arrange
        var manager = new ModelStateManager<TestModel>();
        var model = new TestModel { Name = "Test", Value = 42 };
        manager.Set(model);

        // Act
        manager.Clear();

        // Assert
        manager.Model.Should().BeNull();
    }

    [Test]
    public void Clear_WhenCalled_ShouldTriggerOnStateChangedEvent()
    {
        // Arrange
        var manager = new ModelStateManager<TestModel>();
        var model = new TestModel { Name = "Test" };
        manager.Set(model);

        var eventTriggered = false;
        manager.OnStateChanged += (sender, args) => eventTriggered = true;

        // Act
        manager.Clear();

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [Test]
    public void New_WhenCalled_ShouldCreateNewModelInstance()
    {
        // Arrange
        var manager = new ModelStateManager<TestModel>();

        // Act
        manager.New();

        // Assert
        manager.Model.Should().NotBeNull();
        manager.Model.Should().BeOfType<TestModel>();
        manager.Model!.Name.Should().Be(string.Empty);
        manager.Model.Value.Should().Be(0);
    }

    [Test]
    public void New_WhenCalled_ShouldTriggerOnStateChangedEvent()
    {
        // Arrange
        var manager = new ModelStateManager<TestModel>();
        var eventTriggered = false;
        manager.OnStateChanged += (sender, args) => eventTriggered = true;

        // Act
        manager.New();

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [Test]
    public void New_WhenCalledMultipleTimes_ShouldCreateDifferentInstances()
    {
        // Arrange
        var manager = new ModelStateManager<TestModel>();

        // Act
        manager.New();
        var firstInstance = manager.Model;
        manager.New();
        var secondInstance = manager.Model;

        // Assert
        firstInstance.Should().NotBeNull();
        secondInstance.Should().NotBeNull();
        firstInstance.Should().NotBeSameAs(secondInstance);
    }

    [Test]
    public void NotifyStateChanged_WhenCalled_ShouldTriggerOnStateChangedEvent()
    {
        // Arrange
        var manager = new ModelStateManager<TestModel>();
        var eventTriggered = false;
        EventArgs? capturedArgs = null;
        object? capturedSender = null;

        manager.OnStateChanged += (sender, args) =>
        {
            eventTriggered = true;
            capturedSender = sender;
            capturedArgs = args;
        };

        // Act
        manager.NotifyStateChanged();

        // Assert
        eventTriggered.Should().BeTrue();
        capturedSender.Should().BeSameAs(manager);
        capturedArgs.Should().Be(EventArgs.Empty);
    }

    [Test]
    public void NotifyStateChanged_WithoutSubscribers_ShouldNotThrow()
    {
        // Arrange
        var manager = new ModelStateManager<TestModel>();

        // Act & Assert
        var action = () => manager.NotifyStateChanged();
        action.Should().NotThrow();
    }

    [Test]
    public void OnStateChanged_WithMultipleSubscribers_ShouldNotifyAll()
    {
        // Arrange
        var manager = new ModelStateManager<TestModel>();
        var firstEventTriggered = false;
        var secondEventTriggered = false;

        manager.OnStateChanged += (sender, args) => firstEventTriggered = true;
        manager.OnStateChanged += (sender, args) => secondEventTriggered = true;

        // Act
        manager.NotifyStateChanged();

        // Assert
        firstEventTriggered.Should().BeTrue();
        secondEventTriggered.Should().BeTrue();
    }

    [Test]
    public void StateModelManager_OperationsSequence_ShouldWorkCorrectly()
    {
        // Arrange
        var manager = new ModelStateManager<TestModel>();
        var eventCount = 0;
        manager.OnStateChanged += (sender, args) => eventCount++;

        // Act & Assert
        // Initial state
        manager.Model.Should().BeNull();
        eventCount.Should().Be(0);

        // Create new instance
        manager.New();
        manager.Model.Should().NotBeNull();
        eventCount.Should().Be(1);

        // Set specific model
        var specificModel = new TestModel { Name = "Specific", Value = 123 };
        manager.Set(specificModel);
        manager.Model.Should().BeSameAs(specificModel);
        eventCount.Should().Be(2);

        // Clear model
        manager.Clear();
        manager.Model.Should().BeNull();
        eventCount.Should().Be(3);

        // Manual notification
        manager.NotifyStateChanged();
        manager.Model.Should().BeNull();
        eventCount.Should().Be(4);
    }

    [Test]
    public void OnStateChanged_EventUnsubscription_ShouldWork()
    {
        // Arrange
        var manager = new ModelStateManager<TestModel>();
        var eventTriggered = false;

        void EventHandler(object? sender, EventArgs args) => eventTriggered = true;

        manager.OnStateChanged += EventHandler;

        // Act - trigger event with subscription
        manager.NotifyStateChanged();
        var firstTrigger = eventTriggered;

        // Reset and unsubscribe
        eventTriggered = false;
        manager.OnStateChanged -= EventHandler;

        // Trigger event after unsubscription
        manager.NotifyStateChanged();
        var secondTrigger = eventTriggered;

        // Assert
        firstTrigger.Should().BeTrue();
        secondTrigger.Should().BeFalse();
    }

    // Test with a model that has complex initialization
    private class ComplexModel
    {
        public List<string> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Test]
    public void StateModelManager_WithComplexModel_ShouldWorkCorrectly()
    {
        // Arrange
        var manager = new ModelStateManager<ComplexModel>();

        // Act
        manager.New();

        // Assert
        manager.Model.Should().NotBeNull();
        manager.Model!.Items.Should().NotBeNull();
        manager.Model.Items.Should().BeEmpty();
        manager.Model.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }
}
