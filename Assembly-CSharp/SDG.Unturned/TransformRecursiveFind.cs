using UnityEngine;

namespace SDG.Unturned;

public static class TransformRecursiveFind
{
    public static Transform FindChildRecursive(this Transform parent, string name)
    {
        int childCount = parent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name)
            {
                return child;
            }
            if (child.childCount != 0)
            {
                child = child.FindChildRecursive(name);
                if (child != null)
                {
                    return child;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Same as FindChildRecursive, but skip specific child.
    /// </summary>
    public static Transform FindChildRecursiveWithExclusion(this Transform parent, string name, Transform excludedChild)
    {
        int childCount = parent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child == excludedChild)
            {
                continue;
            }
            if (child.name == name)
            {
                return child;
            }
            if (child.childCount != 0)
            {
                child = child.FindChildRecursive(name);
                if (child != null)
                {
                    return child;
                }
            }
        }
        return null;
    }
}
