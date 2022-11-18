using UnityEngine;

namespace SDG.Unturned;

public class BarricadeData
{
    private Barricade _barricade;

    public Vector3 point;

    public byte angle_x;

    public byte angle_y;

    public byte angle_z;

    public ulong owner;

    public ulong group;

    public uint objActiveDate;

    public Barricade barricade => _barricade;

    public uint instanceID { get; private set; }

    public BarricadeData(Barricade newBarricade, Vector3 newPoint, byte newAngle_X, byte newAngle_Y, byte newAngle_Z, ulong newOwner, ulong newGroup, uint newObjActiveDate, uint newInstanceID)
    {
        _barricade = newBarricade;
        point = newPoint;
        angle_x = newAngle_X;
        angle_y = newAngle_Y;
        angle_z = newAngle_Z;
        owner = newOwner;
        group = newGroup;
        objActiveDate = newObjActiveDate;
        instanceID = newInstanceID;
    }
}
