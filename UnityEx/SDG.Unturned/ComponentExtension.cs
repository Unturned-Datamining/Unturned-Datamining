using UnityEngine;

namespace SDG.Unturned;

public static class ComponentExtension
{
    public static string GetSceneHierarchyPath(this Component component)
    {
        if (!(component != null))
        {
            return null;
        }
        return component.transform.GetSceneHierarchyPath();
    }
}
