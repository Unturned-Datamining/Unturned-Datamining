namespace SDG.Unturned;

public class NPCAchievementReward : INPCReward
{
    public string id { get; protected set; }

    public override void GrantReward(Player player)
    {
        player.sendAchievementUnlocked(id);
    }

    public NPCAchievementReward(string newID, string newText)
        : base(newText)
    {
        id = newID;
    }
}
