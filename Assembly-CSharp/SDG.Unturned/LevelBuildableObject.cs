using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class LevelBuildableObject
{
    private static List<Collider> colliders = new List<Collider>();

    public Vector3 point;

    public Quaternion rotation;

    private Transform _transform;

    private ushort _id;

    private ItemAsset _asset;

    public Transform transform => _transform;

    public ushort id => _id;

    public ItemAsset asset => _asset;

    public bool isEnabled { get; private set; }

    public void enable()
    {
        isEnabled = true;
        if (transform != null)
        {
            transform.gameObject.SetActive(value: true);
        }
    }

    public void disable()
    {
        isEnabled = false;
        if (transform != null)
        {
            transform.gameObject.SetActive(value: false);
        }
    }

    public void destroy()
    {
        if (transform != null)
        {
            Object.Destroy(transform.gameObject);
        }
    }

    public LevelBuildableObject(Vector3 newPoint, Quaternion newRotation, ushort newID)
    {
        point = newPoint;
        rotation = newRotation;
        _id = newID;
        _asset = Assets.find(EAssetType.ITEM, id) as ItemAsset;
        if (asset == null || asset.id != id)
        {
            _asset = Assets.find(EAssetType.ITEM, id) as ItemAsset;
            if (asset == null)
            {
                return;
            }
        }
        if (!Level.isEditor)
        {
            return;
        }
        ItemBarricadeAsset itemBarricadeAsset = asset as ItemBarricadeAsset;
        ItemStructureAsset itemStructureAsset = asset as ItemStructureAsset;
        GameObject gameObject = null;
        if (itemBarricadeAsset != null)
        {
            gameObject = itemBarricadeAsset.barricade;
        }
        else if (itemStructureAsset != null)
        {
            gameObject = itemStructureAsset.structure;
        }
        if (!(gameObject != null))
        {
            return;
        }
        GameObject gameObject2 = Object.Instantiate(gameObject, newPoint, newRotation);
        _transform = gameObject2.transform;
        gameObject2.name = id.ToString();
        if (transform.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rigidbody = transform.gameObject.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
        }
        transform.gameObject.SetActive(value: false);
        colliders.Clear();
        transform.GetComponentsInChildren(includeInactive: true, colliders);
        for (int i = 0; i < colliders.Count; i++)
        {
            if (colliders[i].gameObject.layer != 27 && colliders[i].gameObject.layer != 28)
            {
                Object.Destroy(colliders[i].gameObject);
            }
        }
    }
}
