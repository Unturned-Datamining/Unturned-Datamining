using System.Collections.Generic;

namespace SDG.Unturned;

public class PlayerQuestComparator : IComparer<PlayerQuest>
{
    public int Compare(PlayerQuest a, PlayerQuest b)
    {
        return a.id - b.id;
    }
}
