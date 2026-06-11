using System;
using System.Collections.Generic;

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

    /// <summary>
    /// Allows modders to override which links are shown in the main menu escape menu.
    /// </summary>
    public List<CustomMenuLink> Custom_Menu_Links;
}
