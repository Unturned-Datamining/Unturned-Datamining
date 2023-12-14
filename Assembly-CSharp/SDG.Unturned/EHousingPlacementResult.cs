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
    MissingGround,
    /// <summary>
    /// Pillars can be partly underground or inside a designated allowed underground area. Otherwise,
    /// if the very top of the pillar is underground placement is blocked. (public issue #4250)
    /// </summary>
    ObstructedByGround
}
