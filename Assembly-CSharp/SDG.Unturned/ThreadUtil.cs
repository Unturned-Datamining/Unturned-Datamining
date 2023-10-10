using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace SDG.Unturned;

public class ThreadUtil : MonoBehaviour
{
    private struct WorkItem
    {
        public WaitCallback callback;

        public object state;
    }

    private ConcurrentQueue<WorkItem> workItems = new ConcurrentQueue<WorkItem>();

    private static ThreadUtil instance;

    public static Thread gameThread { get; private set; }

    internal static void QueueGameThreadWorkItem(WaitCallback callback, object state)
    {
        instance.workItems.Enqueue(new WorkItem
        {
            callback = callback,
            state = state
        });
    }

    public static void setupGameThread()
    {
        if (gameThread == null)
        {
            gameThread = Thread.CurrentThread;
            GameObject obj = new GameObject("ThreadUtil");
            UnityEngine.Object.DontDestroyOnLoad(obj);
            obj.hideFlags = HideFlags.HideAndDontSave;
            instance = obj.AddComponent<ThreadUtil>();
            return;
        }
        throw new Exception("gameThread has already been setup");
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

    private void Update()
    {
        if (workItems.TryDequeue(out var result) && result.callback != null)
        {
            result.callback(result.state);
        }
    }
}
