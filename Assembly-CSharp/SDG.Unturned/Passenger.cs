using UnityEngine;

namespace SDG.Unturned;

public class Passenger
{
    public SteamPlayer player;

    public TurretInfo turret;

    private Transform _seat;

    private Transform _obj;

    public VehicleTurretEventHook turretEventHook;

    private Transform _turretYaw;

    private Transform _turretPitch;

    private Transform _turretAim;

    public byte[] state;

    internal CapsuleCollider collider;

    public Transform seat => _seat;

    public Transform obj => _obj;

    public Quaternion rotationYaw { get; private set; }

    public Transform turretYaw => _turretYaw;

    public Quaternion rotationPitch { get; private set; }

    public Transform turretPitch => _turretPitch;

    public Transform turretAim => _turretAim;

    public Passenger(Transform newSeat, Transform newObj, Transform newTurretYaw, Transform newTurretPitch, Transform newTurretAim)
    {
        _seat = newSeat;
        _obj = newObj;
        _turretYaw = newTurretYaw;
        _turretPitch = newTurretPitch;
        _turretAim = newTurretAim;
        if (turretYaw != null)
        {
            rotationYaw = turretYaw.localRotation;
        }
        if (turretPitch != null)
        {
            rotationPitch = turretPitch.localRotation;
        }
    }
}
