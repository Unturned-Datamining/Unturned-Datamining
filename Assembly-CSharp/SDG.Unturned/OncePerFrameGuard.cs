using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// True once per frame, false otherwise.
/// </summary>
public struct OncePerFrameGuard
{
    private int consumedFrameNumber;

    public bool HasBeenConsumed => Time.frameCount == consumedFrameNumber;

    public bool Consume()
    {
        int frameCount = Time.frameCount;
        if (frameCount > consumedFrameNumber)
        {
            consumedFrameNumber = frameCount;
            return true;
        }
        return false;
    }
}
