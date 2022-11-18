using System;
using UnityEngine;

namespace SDG.Unturned;

public class Bundles : MonoBehaviour
{
    private static bool _isInitialized;

    public static bool isInitialized => _isInitialized;

    public static Bundle getBundle(string path)
    {
        return getBundle(path, prependRoot: true);
    }

    public static Bundle getBundle(string path, bool prependRoot)
    {
        return new Bundle(path, prependRoot);
    }

    [Obsolete]
    public static Bundle getBundle(string path, bool prependRoot, bool loadFromResources)
    {
        return getBundle(path, prependRoot);
    }

    private void Awake()
    {
        if (isInitialized)
        {
            UnityEngine.Object.Destroy(base.gameObject);
            return;
        }
        _isInitialized = true;
        UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
    }
}
