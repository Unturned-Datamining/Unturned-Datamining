using System.Collections.Generic;

namespace SDG.Unturned;

public class MapsStatusData
{
    /// <summary>
    /// Maps not installed by default, but recommended from maps list.
    /// </summary>
    public List<CuratedMapLink> Curated_Map_Links;

    /// <summary>
    /// Maps to install to automatically.
    /// Used early in startup to hopefully install before reaching main menu.
    /// </summary>
    public List<AutoSubscribeMap> Auto_Subscribe;

    public MapsStatusData()
    {
        Curated_Map_Links = new List<CuratedMapLink>();
        Auto_Subscribe = new List<AutoSubscribeMap>();
    }
}
