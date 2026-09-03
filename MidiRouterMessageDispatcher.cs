namespace midi_router;

internal static class MidiRouterMessageDispatcher
{
    public static void Enqueue(Action callback, Action<Exception>? onError = null)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                callback();
            }
            catch (Exception exception)
            {
                onError?.Invoke(exception);
            }
        });
    }
}
