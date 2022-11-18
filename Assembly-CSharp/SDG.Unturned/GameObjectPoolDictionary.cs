using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class GameObjectPoolDictionary
{
    private Dictionary<GameObject, GameObjectPool> pools;

    public PoolReference Instantiate(GameObject prefab)
    {
        return Instantiate(prefab, Vector3.zero, Quaternion.identity);
    }

    public PoolReference Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!pools.TryGetValue(prefab, out var value))
        {
            value = new GameObjectPool(prefab);
            pools.Add(prefab, value);
        }
        return value.Instantiate(position, rotation);
    }

    public void Instantiate(GameObject prefab, string name, int count)
    {
        if (!pools.TryGetValue(prefab, out var value))
        {
            value = new GameObjectPool(prefab, count);
            pools.Add(prefab, value);
        }
        GameObject[] array = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            GameObject gameObject = value.Instantiate().gameObject;
            gameObject.name = name;
            array[i] = gameObject;
        }
        for (int j = 0; j < count; j++)
        {
            value.Destroy(array[j].GetComponent<PoolReference>());
        }
    }

    public void Destroy(GameObject element)
    {
        if (!(element == null))
        {
            PoolReference component = element.GetComponent<PoolReference>();
            if (component == null || component.pool == null)
            {
                Object.Destroy(element);
            }
            else
            {
                component.pool.Destroy(component);
            }
        }
    }

    public void Destroy(GameObject element, float t)
    {
        if (!(element == null))
        {
            PoolReference component = element.GetComponent<PoolReference>();
            if (component == null || component.pool == null)
            {
                Object.Destroy(element);
            }
            else
            {
                component.DestroyIntoPool(t);
            }
        }
    }

    public void DestroyAll()
    {
        foreach (GameObjectPool value in pools.Values)
        {
            value.DestroyAll();
        }
    }

    public void DestroyAllMatchingPrefab(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out var value))
        {
            value.DestroyAll();
        }
    }

    public GameObjectPoolDictionary()
    {
        pools = new Dictionary<GameObject, GameObjectPool>();
    }
}
