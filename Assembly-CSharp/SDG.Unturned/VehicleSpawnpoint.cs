using UnityEngine;

namespace SDG.Unturned;

public class VehicleSpawnpoint
{
    public byte type;

    private Vector3 _point;

    private float _angle;

    private Transform _node;

    public Vector3 point => _point;

    public float angle => _angle;

    public Transform node => _node;

    public void setEnabled(bool isEnabled)
    {
        node.transform.gameObject.SetActive(isEnabled);
    }

    public VehicleSpawnpoint(byte newType, Vector3 newPoint, float newAngle)
    {
        type = newType;
        _point = newPoint;
        _angle = newAngle;
        if (Level.isEditor)
        {
            _node = ((GameObject)Object.Instantiate(Resources.Load("Edit/Vehicle"))).transform;
            node.name = type.ToString();
            node.position = point;
            node.rotation = Quaternion.Euler(0f, angle, 0f);
            node.GetComponent<Renderer>().material.color = LevelVehicles.tables[type].color;
            node.Find("Arrow").GetComponent<Renderer>().material.color = LevelVehicles.tables[type].color;
        }
    }
}
