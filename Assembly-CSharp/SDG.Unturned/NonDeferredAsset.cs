using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Legacy implementation that preloads assets.
/// </summary>
public struct NonDeferredAsset<T> : IDeferredAsset<T> where T : Object
{
    public T loadedObject;

    public T getOrLoad()
    {
        return loadedObject;
    }

    public NonDeferredAsset(T loadedObject)
    {
        this.loadedObject = loadedObject;
    }
}
