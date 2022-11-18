using System;
using System.Collections;
using UnityEngine;

namespace SDG.Framework.Utilities;

public class TimeUtility : MonoBehaviour
{
    private static TimeUtility singleton;

    public static event UpdateHandler updated;

    public static event UpdateHandler physicsUpdated;

    public static void InvokeAfterDelay(Action callback, float timeSeconds)
    {
        singleton.StartCoroutine(singleton.InternalInvokeAfterDelay(callback, timeSeconds));
    }

    protected virtual void triggerUpdated()
    {
        if (TimeUtility.updated != null)
        {
            TimeUtility.updated();
        }
    }

    protected virtual void Update()
    {
        TimeUtility.updated?.Invoke();
    }

    protected virtual void FixedUpdate()
    {
        TimeUtility.physicsUpdated?.Invoke();
    }

    private IEnumerator InternalInvokeAfterDelay(Action callback, float timeSeconds)
    {
        yield return new WaitForSeconds(timeSeconds);
        callback();
    }

    private void Awake()
    {
        singleton = this;
    }
}
