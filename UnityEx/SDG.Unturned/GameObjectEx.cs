using UnityEngine;

namespace SDG.Unturned;

public static class GameObjectEx
{
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        T val = gameObject.GetComponent<T>();
        if (val == null)
        {
            val = gameObject.AddComponent<T>();
        }
        return val;
    }

    public static RectTransform GetRectTransform(this GameObject gameObject)
    {
        return gameObject.transform as RectTransform;
    }

    public static void SetLayerRecursively(this GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        Transform transform = gameObject.transform;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetLayerRecursively(layer);
        }
    }

    public static void SetTagIfUntaggedRecursively(this GameObject gameObject, string tag)
    {
        if (gameObject.CompareTag("Untagged"))
        {
            gameObject.tag = tag;
        }
        foreach (Transform item in gameObject.transform)
        {
            item.gameObject.SetTagIfUntaggedRecursively(tag);
        }
    }

    public static string GetSceneHierarchyPath(this GameObject gameObject)
    {
        if (!(gameObject != null))
        {
            return null;
        }
        return gameObject.transform.GetSceneHierarchyPath();
    }
}
