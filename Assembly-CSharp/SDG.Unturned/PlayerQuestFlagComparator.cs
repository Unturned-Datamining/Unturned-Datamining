using System.Collections.Generic;

namespace SDG.Unturned;

public class PlayerQuestFlagComparator : IComparer<PlayerQuestFlag>
{
    public int Compare(PlayerQuestFlag a, PlayerQuestFlag b)
    {
        return a.id - b.id;
    }
}
