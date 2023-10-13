using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace SDG.Unturned;

internal class GameThreadQueueUtil : MonoBehaviour
{
    private struct WorkItem
    {
        public WaitCallback callback;

        public object state;
    }

    private ConcurrentQueue<WorkItem> workItems = new ConcurrentQueue<WorkItem>();

    private static GameThreadQueueUtil instance;

    internal static void Setup()
    {
        GameObject obj = new GameObject("ThreadUtil");
        Object.DontDestroyOnLoad(obj);
        obj.hideFlags = HideFlags.HideAndDontSave;
        instance = obj.AddComponent<GameThreadQueueUtil>();
    }

    internal static void QueueGameThreadWorkItem(WaitCallback callback, object state)
    {
        instance.workItems.Enqueue(new WorkItem
        {
            callback = callback,
            state = state
        });
    }

    private void Update()
    {
        if (workItems.TryDequeue(out var result) && result.callback != null)
        {
            result.callback(result.state);
        }
    }
}
