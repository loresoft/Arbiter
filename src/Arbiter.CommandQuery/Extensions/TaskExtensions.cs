using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Extensions;

/// <summary>
/// Provides extension methods for observing tasks that run in the background.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Observes the task in the background without awaiting it and optionally logs any unhandled exception.
    /// </summary>
    /// <remarks>
    /// This overload also supports <see cref="Task{TResult}"/> because it derives from <see cref="Task"/>.
    /// Successful and canceled tasks are not logged.
    /// </remarks>
    /// <param name="task">The task whose completion is observed in the background.</param>
    /// <param name="logger">The optional logger used to record an unhandled exception.</param>
    /// <exception cref="ArgumentNullException"><paramref name="task"/> is <see langword="null"/>.</exception>
    public static void RunInBackground(this Task task, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task.IsCompletedSuccessfully)
            return;

        _ = task.ContinueWith(
            continuationAction: static (completedTask, state) =>
            {
                var logger = state as ILogger;
                var exception = completedTask.Exception?.GetBaseException();

                if (exception is not null && logger is not null)
                    logger.LogError(exception, "An error occurred while running a task in the background: {ErrorMessage}", exception.Message);
            },
            state: logger,
            cancellationToken: CancellationToken.None,
            continuationOptions: TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
            scheduler: TaskScheduler.Default
        );
    }

    /// <summary>
    /// Observes the value task in the background without awaiting it and optionally logs any unhandled exception.
    /// </summary>
    /// <remarks>Successful and canceled value tasks are not logged.</remarks>
    /// <param name="task">The value task whose completion is observed in the background.</param>
    /// <param name="logger">The optional logger used to record an unhandled exception.</param>
    public static void RunInBackground(this ValueTask task, ILogger? logger = null)
    {
        if (task.IsCompletedSuccessfully)
            return;

        task.AsTask().RunInBackground(logger);
    }

    /// <summary>
    /// Observes the value task in the background without awaiting it and optionally logs any unhandled exception.
    /// </summary>
    /// <remarks>Successful and canceled value tasks are not logged.</remarks>
    /// <typeparam name="TResult">The type of value produced by the task.</typeparam>
    /// <param name="task">The value task whose completion is observed in the background.</param>
    /// <param name="logger">The optional logger used to record an unhandled exception.</param>
    public static void RunInBackground<TResult>(this ValueTask<TResult> task, ILogger? logger = null)
    {
        if (task.IsCompletedSuccessfully)
            return;

        task.AsTask().RunInBackground(logger);
    }
}
