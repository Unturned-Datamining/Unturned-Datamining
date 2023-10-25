namespace SDG.Unturned;

/// <summary>
/// Associates a train vehicle ID with the index of a road path to spawn it on.
/// The level only spawns the train if this vehicle ID isn't present in the map yet, so every train on the map has to be different.
/// </summary>
public class LevelTrainAssociation
{
    public ushort VehicleID;

    public ushort RoadIndex;

    public float Min_Spawn_Placement = 0.1f;

    public float Max_Spawn_Placement = 0.9f;
}
