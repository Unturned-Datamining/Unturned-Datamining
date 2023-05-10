using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unturned.UnityEx;

public static class UnityEventEx
{
    public static void TryInvoke(this UnityEvent unityEvent, UnityEngine.Object context)
    {
        try
        {
            unityEvent.Invoke();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, context);
        }
    }
}
