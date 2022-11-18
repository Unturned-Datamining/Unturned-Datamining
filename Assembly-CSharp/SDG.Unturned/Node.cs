using UnityEngine;

namespace SDG.Unturned;

public class Node
{
    protected Vector3 _point;

    protected ENodeType _type;

    protected Transform _model;

    public Vector3 point => _point;

    public ENodeType type => _type;

    public Transform model => _model;

    public void move(Vector3 newPoint)
    {
        _point = newPoint;
        if (_model != null)
        {
            _model.position = point;
        }
    }

    public void setEnabled(bool isEnabled)
    {
        if (_model != null)
        {
            _model.gameObject.SetActive(isEnabled);
        }
    }

    public void remove()
    {
        if (_model != null)
        {
            Object.Destroy(_model.gameObject);
            _model = null;
        }
    }
}
