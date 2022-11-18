using UnityEngine;

namespace SDG.Unturned;

public struct RateLimitedAction
{
    public float performedRealtime;

    public float realtimeSincePerformed => Time.realtimeSinceStartup - performedRealtime;

    public bool hasIntervalPassed(float interval)
    {
        return realtimeSincePerformed > interval;
    }

    public bool throttle(float interval)
    {
        if (hasIntervalPassed(interval))
        {
            performedRealtime = Time.realtimeSinceStartup;
            return false;
        }
        return true;
    }
}
