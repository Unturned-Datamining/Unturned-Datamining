namespace SDG.Unturned;

internal enum EHousingPlacementResult
{
    Success,
    MissingSlot,
    Obstructed,
    MissingPillar,
    /// <summary>
    /// Floors must be placed touching the terrain, or a fake-terrain object like a grassy cliff model.
    /// </summary>
    MissingGround
}
