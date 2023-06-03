using Avalonia.Threading;

namespace NxEditor.Helpers;
public static class TaskExtension
{
    /// <summary>
    /// Synchronously awaits a <see cref="Task"/> function on the UI thread
    /// </summary>
    /// <param name="task"></param>
    public static void WaitSynchronously(this Task task)
    {
        using var source = new CancellationTokenSource();
        task.ContinueWith(t => source.Cancel(), TaskScheduler.FromCurrentSynchronizationContext());
        Dispatcher.UIThread.MainLoop(source.Token);
    }

    /// <summary>
    /// Synchronously awaits a <see cref="Task{TResult}"/> function on the UI thread and returns the task result
    /// </summary>
    /// <param name="task"></param>
    public static TResult WaitSynchronously<TResult>(this Task<TResult> task)
    {
        using var source = new CancellationTokenSource();
        task.ContinueWith(t => source.Cancel(), TaskScheduler.FromCurrentSynchronizationContext());
        Dispatcher.UIThread.MainLoop(source.Token);
        return task.Result;
    }
}
