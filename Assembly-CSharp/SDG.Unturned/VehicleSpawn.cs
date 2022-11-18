namespace SDG.Unturned;

public class VehicleSpawn
{
    private ushort _vehicle;

    public ushort vehicle => _vehicle;

    public VehicleSpawn(ushort newVehicle)
    {
        _vehicle = newVehicle;
    }
}
