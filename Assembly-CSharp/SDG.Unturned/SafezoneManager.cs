using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class SafezoneManager : MonoBehaviour
{
    private static List<SafezoneBubble> bubbles;

    public static bool checkPointValid(Vector3 point)
    {
        for (int i = 0; i < bubbles.Count; i++)
        {
            SafezoneBubble safezoneBubble = bubbles[i];
            if ((safezoneBubble.origin - point).sqrMagnitude < safezoneBubble.sqrRadius)
            {
                return false;
            }
        }
        return true;
    }

    public static SafezoneBubble registerBubble(Vector3 origin, float radius)
    {
        SafezoneBubble safezoneBubble = new SafezoneBubble(origin, radius * radius);
        bubbles.Add(safezoneBubble);
        return safezoneBubble;
    }

    public static void deregisterBubble(SafezoneBubble bubble)
    {
        bubbles.Remove(bubble);
    }

    private void onLevelLoaded(int level)
    {
        bubbles = new List<SafezoneBubble>();
    }

    private void Start()
    {
        Level.onPrePreLevelLoaded = (PrePreLevelLoaded)Delegate.Combine(Level.onPrePreLevelLoaded, new PrePreLevelLoaded(onLevelLoaded));
    }
}
