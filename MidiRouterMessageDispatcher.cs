namespace midi_router;

internal static class MidiRouterMessageDispatcher
{
    public static void Enqueue(Action callback)
    {
        ThreadPool.QueueUserWorkItem(_ => callback());
    }
}
