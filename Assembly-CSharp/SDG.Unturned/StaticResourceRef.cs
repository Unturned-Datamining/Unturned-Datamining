using UnityEngine;

namespace SDG.Unturned;

public class StaticResourceRef<T> where T : Object
{
    private string path;

    private T asset;

    private bool needsLoad;

    public T GetOrLoad()
    {
        if (needsLoad)
        {
            needsLoad = false;
            asset = Resources.Load<T>(path);
            if ((Object)asset == (Object)null)
            {
                UnturnedLog.error("Missing resource {0} ({1})", path, typeof(T));
            }
        }
        return asset;
    }

    public StaticResourceRef(string path)
    {
        this.path = path;
        asset = null;
        needsLoad = true;
    }

    public static implicit operator T(StaticResourceRef<T> resource)
    {
        return resource.GetOrLoad();
    }
}
