namespace SDG.Unturned;

public class AchievementStatusData
{
    /// <summary>
    /// Names of achievements that can be granted by NPC rewards.
    /// </summary>
    public string[] NPC_Achievement_IDs;

    public bool canBeGrantedByNPC(string id)
    {
        string[] nPC_Achievement_IDs = NPC_Achievement_IDs;
        for (int i = 0; i < nPC_Achievement_IDs.Length; i++)
        {
            if (string.Equals(nPC_Achievement_IDs[i], id))
            {
                return true;
            }
        }
        return false;
    }
}
