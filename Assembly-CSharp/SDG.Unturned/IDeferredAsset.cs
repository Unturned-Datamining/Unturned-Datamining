using UnityEngine;

namespace SDG.Unturned;

public interface IDeferredAsset<T> where T : Object
{
    T getOrLoad();
}
