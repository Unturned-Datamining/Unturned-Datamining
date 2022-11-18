using UnityEngine;

namespace SDG.Unturned;

public class ItemSpawnpoint
{
    public byte type;

    private Vector3 _point;

    private Transform _node;

    public Vector3 point => _point;

    public Transform node => _node;

    public void setEnabled(bool isEnabled)
    {
        node.transform.gameObject.SetActive(isEnabled);
    }

    public ItemSpawnpoint(byte newType, Vector3 newPoint)
    {
        type = newType;
        _point = newPoint;
        if (Level.isEditor)
        {
            _node = ((GameObject)Object.Instantiate(Resources.Load("Edit/Item"))).transform;
            node.name = type.ToString();
            node.position = point;
            node.GetComponent<Renderer>().material.color = LevelItems.tables[type].color;
        }
    }
}
