using UnityEngine;

namespace SDG.Unturned;

public struct DeferredMasterAsset<T> : IDeferredAsset<T> where T : Object
{
    public MasterBundle masterBundle;

    public string name;

    public LoadedAssetDeferredCallback<T> callback;

    public T loadedObject;

    public bool hasLoaded;

    public T getOrLoad()
    {
        if (!hasLoaded)
        {
            hasLoaded = true;
            loadedObject = masterBundle.load<T>(name);
            callback?.Invoke(loadedObject);
        }
        return loadedObject;
    }
}
