using System;

namespace SDG.Unturned;

public class MenuStatusData
{
    /// <summary>
    /// Name of promo level to additively load.
    /// </summary>
    public string PromoLevel;

    /// <summary>
    /// UTC when to begin load promo level.
    /// </summary>
    public DateTime PromoStart;

    /// <summary>
    /// UTC when to stop loading promo level.
    /// </summary>
    public DateTime PromoEnd;
}
