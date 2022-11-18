using UnityEngine;

namespace SDG.Unturned;

internal struct ClientMovementInput
{
    public uint frameNumber;

    public bool crouch;

    public bool prone;

    public bool sprint;

    public int input_x;

    public int input_y;

    public bool jump;

    public Quaternion rotation;

    public Quaternion aimRotation;
}
