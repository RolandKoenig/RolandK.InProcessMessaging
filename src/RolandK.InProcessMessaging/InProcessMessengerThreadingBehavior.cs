﻿namespace RolandK.InProcessMessaging;

/// <summary>
/// This enum describes the threading behavior of a Messenger.
/// </summary>
public enum InProcessMessengerThreadingBehavior
{
    /// <summary>
    /// Ignore threading. Just use default lock on registration list.
    /// </summary>
    Ignore,

    /// <summary>
    /// Ensures that the main SynchronizationContext is set when 
    /// this Messenger is used on synchronous calls.
    /// </summary>
    EnsureMainSyncContextOnSyncCalls
}