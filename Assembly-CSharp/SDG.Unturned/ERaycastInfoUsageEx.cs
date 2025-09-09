namespace SDG.Unturned;

public static class ERaycastInfoUsageEx
{
    /// <summary>
    /// Nelson 2025-09-09: changing to only require bounds test when usage will
    /// apply damage because Detonator usage provides the barricade root transform
    /// which may not have a collider. (Public issue #5202.)
    /// </summary>
    public static bool RequiresBarricadeBoundsTest(this ERaycastInfoUsage usage)
    {
        if (usage == ERaycastInfoUsage.Punch || (uint)(usage - 2) <= 2u)
        {
            return true;
        }
        return false;
    }
}
