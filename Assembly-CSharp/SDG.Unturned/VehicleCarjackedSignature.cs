using UnityEngine;

namespace SDG.Unturned;

public delegate void VehicleCarjackedSignature(InteractableVehicle vehicle, Player instigatingPlayer, ref bool allow, ref Vector3 force, ref Vector3 torque);
