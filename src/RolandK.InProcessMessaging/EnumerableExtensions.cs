using System.Collections.Generic;

namespace RolandK.InProcessMessaging;

public static class EnumerableExtensions
{
    public static void UnsubscribeAll(this IEnumerable<MessageSubscription> subscriptions)
    {
        foreach (var actSubscription in subscriptions)
        {
            actSubscription.Unsubscribe();
        }
    }
}