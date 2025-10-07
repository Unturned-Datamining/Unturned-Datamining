namespace SDG.Unturned;

/// <summary>
/// Nelson 2025-10-06: better late than never. Current indices into the gun state array (and
/// other item state arrays, for that matter) being unnamed makes it hard to read. Updates to
/// gun-related code should prefer using these names.
/// </summary>
public static class GunStateIndices
{
    public const int SIGHT_ID = 0;

    public const int TACTICAL_ID = 2;

    public const int GRIP_ID = 4;

    public const int BARREL_ID = 6;

    public const int MAGAZINE_ID = 8;

    public const int AMMO = 10;

    public const int FIREMODE = 11;

    public const int TACTICAL_ACTIVE = 12;

    public const int SIGHT_QUALITY = 13;

    public const int TACTICAL_QUALITY = 14;

    public const int GRIP_QUALITY = 15;

    public const int BARREL_QUALITY = 16;

    public const int MAGAZINE_QUALITY = 17;
}
