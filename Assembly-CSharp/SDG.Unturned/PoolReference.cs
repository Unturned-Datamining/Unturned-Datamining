using SDG.Framework.Utilities;
using UnityEngine;

namespace SDG.Unturned;

public class PoolReference : MonoBehaviour
{
    public GameObjectPool pool;

    public bool inPool;

    public bool excludeFromDestroyAll;

    private void InternalDestroyIntoPool()
    {
        if (pool == null)
        {
            Object.Destroy(base.gameObject);
        }
        else
        {
            pool.Destroy(this);
        }
    }

    public void DestroyIntoPool(float t)
    {
        if (pool == null)
        {
            Object.Destroy(base.gameObject, t);
        }
        else if (base.gameObject.activeInHierarchy)
        {
            TimeUtility.InvokeAfterDelay(InternalDestroyIntoPool, t);
        }
        else
        {
            pool.Destroy(this);
        }
    }
}
