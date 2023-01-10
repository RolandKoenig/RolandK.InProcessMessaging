using System;
using System.Threading;
using System.Threading.Tasks;

namespace RolandK.InProcessMessaging.Extensions;

internal static class SynchronizationContextExtensions
{
    /// <summary>
    /// Posts the given action to the given SynchronizationContext also if it is null.
    /// If it is null, then a new task will be started.
    /// </summary>
    /// <param name="syncContext">The context to send the action to.</param>
    /// <param name="actionToPost">The action to be executed on the target thread.</param>
    public static void PostAlsoIfNull(
        this SynchronizationContext? syncContext, 
        Action actionToPost)
    {
        if (syncContext != null) { syncContext.Post((_) => actionToPost(), null); }
        else
        {
            Task.Factory.StartNew(actionToPost);
        }
    }

    /// <summary>
    /// Post the given action in an async manner to the given SynchronizationContext.
    /// </summary>
    /// <param name="syncContext">The target SynchronizationContext.</param>
    /// <param name="actionToPost">The action to be executed on the target thread.</param>
    public static Task PostAlsoIfNullAsync(
        this SynchronizationContext? syncContext, 
        Action actionToPost)
    {
        TaskCompletionSource<object?> completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        syncContext.PostAlsoIfNull(() =>
            {
                try
                {
                    actionToPost();
                    completionSource.SetResult(null);
                }
                catch (Exception ex)
                {
                    completionSource.SetException(ex);
                }
            });
        return completionSource.Task;
    }
}