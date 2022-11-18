using UnityEngine;

namespace SDG.Unturned;

public class TrainCar
{
    public float trackPositionOffset;

    public Vector3 currentFrontPosition;

    public Vector3 currentFrontNormal;

    public Vector3 currentFrontDirection;

    public Vector3 currentBackPosition;

    public Vector3 currentBackNormal;

    public Vector3 currentBackDirection;

    public Transform root;

    public Transform trackFront;

    public Transform trackBack;
}
