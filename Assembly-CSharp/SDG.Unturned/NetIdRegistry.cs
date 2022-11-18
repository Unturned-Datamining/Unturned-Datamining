using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace SDG.Unturned;

public static class NetIdRegistry
{
    private static List<Transform> pathTransforms = new List<Transform>(16);

    private static StringBuilder pathStringBuilder = new StringBuilder(256);

    private static uint counter;

    private static Dictionary<NetId, object> pairings = new Dictionary<NetId, object>();

    private static Dictionary<Transform, NetId> transformPairings = new Dictionary<Transform, NetId>();

    public static NetId Claim()
    {
        return new NetId(++counter);
    }

    public static NetId ClaimBlock(uint size)
    {
        NetId result = new NetId(counter + 1);
        counter += size;
        return result;
    }

    public static object Get(NetId key)
    {
        pairings.TryGetValue(key, out var value);
        return value;
    }

    public static T Get<T>(NetId key) where T : class
    {
        return Get(key) as T;
    }

    public static void Assign(NetId key, object value)
    {
        if (pairings.TryGetValue(key, out var value2))
        {
            if (value == value2)
            {
                UnturnedLog.error($"Net id {key} was already assigned to {value}");
            }
            else
            {
                UnturnedLog.error($"Net id {key} was previously assigned to {value2}, reassigning to {value}");
            }
            pairings[key] = value;
        }
        else
        {
            pairings.Add(key, value);
        }
    }

    public static void AssignTransform(NetId key, Transform value)
    {
        object value2;
        bool flag = pairings.TryGetValue(key, out value2);
        NetId value3;
        bool flag2 = transformPairings.TryGetValue(value, out value3);
        if (flag && flag2)
        {
            if (value == value2)
            {
                if (key == value3)
                {
                    UnturnedLog.error($"Net id {key} and transform {value} were already assigned");
                }
                else
                {
                    UnturnedLog.error($"Net id {key} was previously assigned to transform {value}, but transform was previously assigned to net id {value3}, reassigning");
                }
            }
            else if (key == value3)
            {
                UnturnedLog.error($"Transform {value} was previously assigned to net id {key}, but net id was previously assigned to transform {value2}, reassigning");
            }
            else
            {
                UnturnedLog.error($"Net id {key} was previously assigned to {value2} and transform {value} was previously assigned to {value3}, reassigning");
            }
            pairings[key] = value;
            transformPairings[value] = key;
        }
        else if (flag)
        {
            if (value == value2)
            {
                UnturnedLog.error($"Net id {key} was already assigned to transform {value}");
            }
            else
            {
                UnturnedLog.error($"Net id {key} was previously assigned to {value2}, reassigning to transform {value}");
            }
            pairings[key] = value;
            transformPairings.Add(value, key);
        }
        else if (flag2)
        {
            if (key == value3)
            {
                UnturnedLog.error($"Transform {value} was already assigned to net id {key}");
            }
            else
            {
                UnturnedLog.error($"Transform {value} was previously assigned to net id {value3}, reassigning to net id {key}");
            }
            pairings.Add(key, value);
            transformPairings[value] = key;
        }
        else
        {
            pairings.Add(key, value);
            transformPairings.Add(value, key);
        }
    }

    public static bool Release(NetId key)
    {
        return pairings.Remove(key);
    }

    public static void ReleaseTransform(NetId key, Transform value)
    {
        pairings.Remove(key);
        transformPairings.Remove(value);
    }

    public static Transform GetTransform(NetId netId, string path)
    {
        Transform transform = Get<Transform>(netId);
        if (transform != null && !string.IsNullOrEmpty(path))
        {
            transform = transform.Find(path);
        }
        return transform;
    }

    public static bool GetTransformNetId(Transform transform, out NetId netId, out string path)
    {
        if (transform == null)
        {
            netId = NetId.INVALID;
            path = null;
            return false;
        }
        if (transformPairings.TryGetValue(transform, out netId))
        {
            path = null;
            return true;
        }
        Transform parent = transform.parent;
        if (parent != null)
        {
            if (transformPairings.TryGetValue(parent, out netId))
            {
                path = transform.name;
                return true;
            }
            Transform parent2 = parent.parent;
            if (parent2 != null)
            {
                pathTransforms.Clear();
                do
                {
                    if (transformPairings.TryGetValue(parent2, out netId))
                    {
                        pathStringBuilder.Length = 0;
                        for (int num = pathTransforms.Count - 1; num >= 0; num--)
                        {
                            pathStringBuilder.Append(pathTransforms[num].name);
                            pathStringBuilder.Append('/');
                        }
                        pathStringBuilder.Append(parent.name);
                        pathStringBuilder.Append('/');
                        pathStringBuilder.Append(transform.name);
                        path = pathStringBuilder.ToString();
                        return true;
                    }
                    pathTransforms.Add(parent2);
                    parent2 = parent2.parent;
                }
                while (parent2 != null);
            }
        }
        netId = NetId.INVALID;
        path = null;
        return false;
    }

    public static void Dump()
    {
        int num = 0;
        foreach (KeyValuePair<NetId, object> pairing in pairings)
        {
            _ = pairing;
            num++;
        }
    }

    public static void Clear()
    {
        counter = 0u;
        pairings.Clear();
        transformPairings.Clear();
    }

    [Conditional("LOG_NET_ID")]
    private static void Log(string message)
    {
        UnturnedLog.info(message);
    }
}
