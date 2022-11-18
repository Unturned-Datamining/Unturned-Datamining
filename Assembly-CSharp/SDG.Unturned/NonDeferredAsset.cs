using UnityEngine;

namespace SDG.Unturned;

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
