using UnityEngine;

namespace SDG.Unturned;

public class PlayerSpawnpoint
{
    private Vector3 _point;

    private float _angle;

    private bool _isAlt;

    private Transform _node;

    public Vector3 point => _point;

    public float angle => _angle;

    public bool isAlt => _isAlt;

    public Transform node => _node;

    public void setEnabled(bool isEnabled)
    {
        node.transform.gameObject.SetActive(isEnabled);
    }

    public PlayerSpawnpoint(Vector3 newPoint, float newAngle, bool newIsAlt)
    {
        _point = newPoint;
        _angle = newAngle;
        _isAlt = newIsAlt;
        if (Level.isEditor)
        {
            _node = ((GameObject)Object.Instantiate(Resources.Load(isAlt ? "Edit/Player_Alt" : "Edit/Player"))).transform;
            node.name = "Player";
            node.position = point;
            node.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}
