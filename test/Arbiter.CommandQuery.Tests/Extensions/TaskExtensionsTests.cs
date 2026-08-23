using Arbiter.CommandQuery.Extensions;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Tests.Extensions;

public class TaskExtensionsTests
{
    [Test]
    public async Task WhenBackgroundTaskFailsThenErrorIsLogged()
    {
        var expected = new InvalidOperationException("Task failed.");
        var logger = new RecordingLogger();

        Task.FromException(expected).RunInBackground(logger);

        var actual = await logger.LoggedException.Task.WaitAsync(TimeSpan.FromSeconds(1));
        actual.Should().BeSameAs(expected);
    }

    [Test]
    public async Task WhenBackgroundValueTaskFailsThenErrorIsLogged()
    {
        var expected = new InvalidOperationException("ValueTask failed.");
        var logger = new RecordingLogger();

        new ValueTask(Task.FromException(expected)).RunInBackground(logger);

        var actual = await logger.LoggedException.Task.WaitAsync(TimeSpan.FromSeconds(1));
        actual.Should().BeSameAs(expected);
    }

    [Test]
    public async Task WhenBackgroundGenericValueTaskFailsThenErrorIsLogged()
    {
        var expected = new InvalidOperationException("ValueTask failed.");
        var logger = new RecordingLogger();

        new ValueTask<int>(Task.FromException<int>(expected)).RunInBackground(logger);

        var actual = await logger.LoggedException.Task.WaitAsync(TimeSpan.FromSeconds(1));
        actual.Should().BeSameAs(expected);
    }

    private sealed class RecordingLogger : ILogger
    {
        public TaskCompletionSource<Exception?> LoggedException { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            LoggedException.TrySetResult(exception);
        }
    }
}
