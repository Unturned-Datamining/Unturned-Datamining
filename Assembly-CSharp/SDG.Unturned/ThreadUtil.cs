using System;
using System.Diagnostics;
using System.Threading;

namespace SDG.Unturned;

public static class ThreadUtil
{
    public static Thread gameThread { get; private set; }

    public static void setupGameThread()
    {
        if (gameThread == null)
        {
            gameThread = Thread.CurrentThread;
            return;
        }
        throw new Exception("gameThread has already been setup");
    }

    public static bool IsGameThread(this Thread thread)
    {
        return thread == gameThread;
    }

    public static void assertIsGameThread()
    {
        if (Thread.CurrentThread != gameThread)
        {
            throw new NotSupportedException("This function should only be called from the game thread. (e.g. from Unity's Update)");
        }
    }

    [Conditional("WITH_GAME_THREAD_ASSERTIONS")]
    internal static void ConditionalAssertIsGameThread()
    {
        if (Thread.CurrentThread != gameThread)
        {
            throw new NotSupportedException("This function should only be called from the game thread. (e.g. from Unity's Update)");
        }
    }
}
