namespace SDG.Unturned;

public class ObjectConfigData
{
    /// <summary>
    /// Scales how long before interactables like fridges automatically close.
    /// </summary>
    public float Binary_State_Reset_Multiplier;

    /// <summary>
    /// Scales how long before sources of fuel in the world are automatically partially refilled.
    /// </summary>
    public float Fuel_Reset_Multiplier;

    /// <summary>
    /// Scales how long before sources of water in the world are automatically partially refilled.
    /// </summary>
    public float Water_Reset_Multiplier;

    /// <summary>
    /// Scales how long before trees, rocks, and bushes in the world grow back.
    /// </summary>
    public float Resource_Reset_Multiplier;

    /// <summary>
    /// Scales number of items dropped by resources like trees and rocks.
    /// </summary>
    public float Resource_Drops_Multiplier;

    /// <summary>
    /// Scales how long before destructible objects (e.g., fences) automatically repair.
    /// </summary>
    public float Rubble_Reset_Multiplier;

    /// <summary>
    /// Should holiday-specific objects be able to drop special items?
    /// For example, whether christmas presents contain guns.
    /// </summary>
    public bool Allow_Holiday_Drops;

    /// <summary>
    /// Should barricades placed on tree stumps prevent the tree from growing back
    /// while the server is running?
    /// </summary>
    public bool Items_Obstruct_Tree_Respawns;

    public ObjectConfigData(EGameMode mode)
    {
        Binary_State_Reset_Multiplier = 1f;
        Fuel_Reset_Multiplier = 1f;
        Water_Reset_Multiplier = 1f;
        Resource_Reset_Multiplier = 1f;
        Resource_Drops_Multiplier = 1f;
        Rubble_Reset_Multiplier = 1f;
        Allow_Holiday_Drops = true;
        Items_Obstruct_Tree_Respawns = true;
    }
}
