using System;
using System.Collections;
using UnityEngine;

namespace SDG.Framework.Utilities;

public class TimeUtility : MonoBehaviour
{
    private static TimeUtility singleton;

    /// <summary>
    /// Equivalent to MonoBehaviour.Update
    /// </summary>
    public static event UpdateHandler updated;

    /// <summary>
    /// Equivalent to MonoBehaviour.FixedUpdate
    /// </summary>
    public static event UpdateHandler physicsUpdated;

    /// <summary>
    /// Useful when caller is not a MonoBehaviour, or coroutine should not be owned by a component which might get
    /// deactivated. For example attached effects destroy timer should happen regardless of parent deactivation.
    /// </summary>
    public static Coroutine InvokeAfterDelay(Action callback, float timeSeconds)
    {
        return singleton.StartCoroutine(singleton.InternalInvokeAfterDelay(callback, timeSeconds));
    }

    /// <summary>
    /// Stop a coroutine started by InvokeAfterDelay.
    /// </summary>
    public static void StaticStopCoroutine(Coroutine routine)
    {
        if (singleton != null)
        {
            singleton.StopCoroutine(routine);
        }
    }

    protected virtual void triggerUpdated()
    {
        TimeUtility.updated?.Invoke();
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
