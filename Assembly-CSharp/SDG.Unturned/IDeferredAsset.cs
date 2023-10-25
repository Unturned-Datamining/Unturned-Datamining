using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Struct interface so that for transient asset bundles (older workshop mods) they can be preloaded
/// and retrieved as-needed, but for master bundles the asset loading can be deferred until needed.
/// </summary>
public interface IDeferredAsset<T> where T : Object
{
    T getOrLoad();
}
