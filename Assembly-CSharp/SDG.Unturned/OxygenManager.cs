using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class OxygenManager : MonoBehaviour
{
    private static List<OxygenBubble> bubbles;

    public static bool checkPointBreathable(Vector3 point)
    {
        for (int i = 0; i < bubbles.Count; i++)
        {
            OxygenBubble oxygenBubble = bubbles[i];
            if (!(oxygenBubble.origin == null) && (oxygenBubble.origin.position - point).sqrMagnitude < oxygenBubble.sqrRadius)
            {
                return true;
            }
        }
        return false;
    }

    public static OxygenBubble registerBubble(Transform origin, float radius)
    {
        OxygenBubble oxygenBubble = new OxygenBubble(origin, radius * radius);
        bubbles.Add(oxygenBubble);
        return oxygenBubble;
    }

    public static void deregisterBubble(OxygenBubble bubble)
    {
        bubbles.Remove(bubble);
    }

    private void onLevelLoaded(int level)
    {
        bubbles = new List<OxygenBubble>();
    }

    private void Start()
    {
        Level.onPrePreLevelLoaded = (PrePreLevelLoaded)Delegate.Combine(Level.onPrePreLevelLoaded, new PrePreLevelLoaded(onLevelLoaded));
    }
}
