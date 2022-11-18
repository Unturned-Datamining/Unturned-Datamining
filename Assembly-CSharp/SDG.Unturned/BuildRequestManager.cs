using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class BuildRequestManager
{
    private struct PendingBuild
    {
        public int handle;

        public Vector3 location;
    }

    private static List<PendingBuild> pendingBuilds = new List<PendingBuild>();

    private static int highestHandleId;

    public static int registerPendingBuild(Vector3 location)
    {
        PendingBuild item = default(PendingBuild);
        item.handle = getUniqueHandle();
        item.location = location;
        pendingBuilds.Add(item);
        return item.handle;
    }

    public static bool canBuildAt(Vector3 location, int ignoreHandle)
    {
        foreach (PendingBuild pendingBuild in pendingBuilds)
        {
            if ((pendingBuild.location - location).sqrMagnitude < 0.01f && pendingBuild.handle != ignoreHandle)
            {
                return false;
            }
        }
        return true;
    }

    public static void finishPendingBuild(ref int handle)
    {
        if (!isValidHandle(handle))
        {
            return;
        }
        int count = pendingBuilds.Count;
        for (int i = 0; i < count; i++)
        {
            if (pendingBuilds[i].handle == handle)
            {
                pendingBuilds.RemoveAtFast(i);
                handle = -1;
                return;
            }
        }
        handle = -1;
    }

    public static bool isValidHandle(int handle)
    {
        return handle > 0;
    }

    private static int getUniqueHandle()
    {
        highestHandleId++;
        return highestHandleId;
    }
}
