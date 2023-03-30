using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class GameObjectPool
{
    private GameObject prefab;

    internal Stack<GameObject> pool;

    internal List<PoolReference> active;

    public PoolReference Instantiate()
    {
        return Instantiate(Vector3.zero, Quaternion.identity);
    }

    public PoolReference Instantiate(Vector3 position, Quaternion rotation)
    {
        if (pool.Count > 0)
        {
            GameObject gameObject = pool.Pop();
            if (gameObject == null)
            {
                return Instantiate(position, rotation);
            }
            gameObject.transform.parent = null;
            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;
            gameObject.transform.localScale = Vector3.one;
            gameObject.SetActive(value: true);
            PoolReference component = gameObject.GetComponent<PoolReference>();
            component.inPool = false;
            component.excludeFromDestroyAll = false;
            active.Add(component);
            return component;
        }
        PoolReference poolReference = Object.Instantiate(prefab, position, rotation).AddComponent<PoolReference>();
        poolReference.pool = this;
        poolReference.inPool = false;
        active.Add(poolReference);
        return poolReference;
    }

    public void Destroy(PoolReference reference)
    {
        if (!(reference == null) && !reference.inPool && reference.pool == this)
        {
            reference.CancelDestroyTimer();
            GameObject gameObject = reference.gameObject;
            gameObject.SetActive(value: false);
            if (gameObject.transform.parent != null)
            {
                EffectManager.UnregisterAttachment(gameObject);
                gameObject.transform.parent = null;
            }
            pool.Push(gameObject);
            active.RemoveFast(reference);
            reference.inPool = true;
            reference.excludeFromDestroyAll = false;
        }
    }

    public void DestroyAll()
    {
        for (int num = active.Count - 1; num >= 0; num--)
        {
            PoolReference poolReference = active[num];
            if (poolReference == null || poolReference.gameObject == null)
            {
                active.RemoveAtFast(num);
            }
            else if (!poolReference.excludeFromDestroyAll)
            {
                poolReference.CancelDestroyTimer();
                GameObject gameObject = poolReference.gameObject;
                gameObject.SetActive(value: false);
                if (gameObject.transform.parent != null)
                {
                    EffectManager.UnregisterAttachment(gameObject);
                    gameObject.transform.parent = null;
                }
                pool.Push(gameObject);
                active.RemoveAtFast(num);
                poolReference.inPool = true;
            }
        }
    }

    public GameObjectPool(GameObject prefab)
        : this(prefab, 1)
    {
    }

    public GameObjectPool(GameObject prefab, int count)
    {
        this.prefab = prefab;
        pool = new Stack<GameObject>(count);
        active = new List<PoolReference>(count);
    }
}
