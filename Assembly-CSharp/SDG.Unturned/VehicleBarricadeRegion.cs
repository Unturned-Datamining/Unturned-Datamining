using UnityEngine;

namespace SDG.Unturned;

public class VehicleBarricadeRegion : BarricadeRegion
{
    internal NetId _netId;

    public InteractableVehicle vehicle { get; private set; }

    public int subvehicleIndex { get; private set; }

    public VehicleBarricadeRegion(Transform parent, InteractableVehicle vehicle, int subvehicleIndex)
        : base(parent)
    {
        this.vehicle = vehicle;
        this.subvehicleIndex = subvehicleIndex;
    }
}
