using System.Collections.Generic;

namespace SDG.Unturned;

public class MapsStatusData
{
    public List<CuratedMapLink> Curated_Map_Links;

    public List<AutoSubscribeMap> Auto_Subscribe;

    public MapsStatusData()
    {
        Curated_Map_Links = new List<CuratedMapLink>();
        Auto_Subscribe = new List<AutoSubscribeMap>();
    }
}
