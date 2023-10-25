using System;
using System.Diagnostics;
using System.Threading;

namespace SDG.Unturned;

public static class ThreadUtil
{
    public static Thread gameThread { get; private set; }

    /// <summary>
    /// Called once by Setup.
    /// </summary>
    public static void setupGameThread()
    {
        if (gameThread == null)
        {
            gameThread = Thread.CurrentThread;
            GameThreadQueueUtil.Setup();
            return;
        }
        throw new Exception("gameThread has already been setup");
    }

    /// <summary>
    /// Extension method for Thread class.
    /// Plugins use this.
    /// I might have accidentally removed it due to zero refs and Pustalorc was mad:
    /// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/discussions/4131
    /// </summary>
    public static bool IsGameThread(this Thread thread)
    {
        return thread == gameThread;
    }

    /// <summary>
    /// Throw an exception if current thread is not the game thread.
    /// </summary>
    public static void assertIsGameThread()
    {
        if (Thread.CurrentThread != gameThread)
        {
            throw new NotSupportedException("This function should only be called from the game thread. (e.g. from Unity's Update)");
        }
    }

    /// <summary>
    /// Only on dedicated server: throw an exception if current thread is not the game thread.
    /// </summary>
    [Conditional("WITH_GAME_THREAD_ASSERTIONS")]
    internal static void ConditionalAssertIsGameThread()
    {
        if (Thread.CurrentThread != gameThread)
        {
            throw new NotSupportedException("This function should only be called from the game thread. (e.g. from Unity's Update)");
        }
    }
}
