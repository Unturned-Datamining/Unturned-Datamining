using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public static class TransformEx
{
    public static T GetOrAddComponent<T>(this Transform transform) where T : Component
    {
        return transform.gameObject.GetOrAddComponent<T>();
    }

    public static Transform GetChildOfParent(this Transform transform, Transform desiredParent)
    {
        Transform transform2 = transform;
        while (true)
        {
            Transform parent = transform2.parent;
            if (parent == null)
            {
                return null;
            }
            if (parent == desiredParent)
            {
                break;
            }
            transform2 = parent;
        }
        return transform2;
    }

    [Obsolete("It looks like this method was a misunderstanding. The engine has a built-in 'root' property.")]
    public static Transform GetRootTransform(this Transform transform)
    {
        return transform.root;
    }

    public static void FindAllChildrenWithName(this Transform transform, string name, List<GameObject> gameObjects)
    {
        foreach (Transform item in transform)
        {
            if (item.name == name)
            {
                gameObjects.Add(item.gameObject);
            }
            item.FindAllChildrenWithName(name, gameObjects);
        }
    }

    public static void FindAllChildrenWithName(this Transform transform, string name, List<Transform> transforms)
    {
        foreach (Transform item in transform)
        {
            if (item.name == name)
            {
                transforms.Add(item);
            }
            item.FindAllChildrenWithName(name, transforms);
        }
    }

    public static Transform FindChild(this IEnumerable<Transform> parentTransforms, string name)
    {
        foreach (Transform parentTransform in parentTransforms)
        {
            if (!(parentTransform == null))
            {
                Transform transform = parentTransform.Find(name);
                if (transform != null)
                {
                    return transform;
                }
            }
        }
        return null;
    }

    public static void DestroyComponentIfExists<T>(this Transform transform) where T : Component
    {
        T component = transform.GetComponent<T>();
        if (component != null)
        {
            UnityEngine.Object.Destroy(component);
        }
    }

    public static void DestroyRigidbody(this Transform transform)
    {
        transform.DestroyComponentIfExists<Rigidbody>();
    }

    public static string GetSceneHierarchyPath(this Transform transform)
    {
        if (transform == null)
        {
            return null;
        }
        string text = transform.name;
        while (true)
        {
            transform = transform.parent;
            if (transform == null)
            {
                break;
            }
            text = transform.name + "/" + text;
        }
        return text;
    }

    public static string DumpChildren(this Transform transform)
    {
        return PrivateDumpChildren(transform, 0);
    }

    public static void SetRotation_RoundIfNearlyAxisAligned(this Transform transform, Quaternion rotation, float tolerance = 0.05f)
    {
        transform.rotation = rotation.GetRoundedIfNearlyAxisAligned(tolerance);
    }

    public static void SetLocalScale_RoundIfNearlyEqualToOne(this Transform transform, Vector3 localScale, float tolerance = 0.001f)
    {
        transform.localScale = localScale.GetRoundedIfNearlyEqualToOne(tolerance);
    }

    public static Quaternion InverseTransformRotation(this Transform transform, Quaternion worldRotation)
    {
        return Quaternion.Inverse(transform.rotation) * worldRotation;
    }

    public static Quaternion TransformRotation(this Transform transform, Quaternion localRotation)
    {
        return transform.rotation * localRotation;
    }

    private static string PrivateDumpChildren(Transform parent, int indentationLevel)
    {
        string text = parent.name;
        for (int i = 0; i < indentationLevel; i++)
        {
            text = "\t" + text;
        }
        foreach (Transform item in parent)
        {
            text += "\n";
            text += PrivateDumpChildren(item, indentationLevel + 1);
        }
        return text;
    }
}
