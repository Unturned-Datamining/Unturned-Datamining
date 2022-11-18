namespace SDG.Unturned;

public class ObjectConfigData
{
    public float Binary_State_Reset_Multiplier;

    public float Fuel_Reset_Multiplier;

    public float Water_Reset_Multiplier;

    public float Resource_Reset_Multiplier;

    public float Resource_Drops_Multiplier;

    public float Rubble_Reset_Multiplier;

    public bool Allow_Holiday_Drops;

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
