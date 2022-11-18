namespace SDG.Unturned;

public class NPCAchievementReward : INPCReward
{
    public string id { get; protected set; }

    public override void grantReward(Player player, bool shouldSend)
    {
        if (shouldSend)
        {
            if (Provider.isServer)
            {
                player.sendAchievementUnlocked(id);
            }
        }
        else if (player.channel.isOwner)
        {
            player.sendAchievementUnlocked(id);
        }
    }

    public NPCAchievementReward(string newID, string newText)
        : base(newText)
    {
        id = newID;
    }
}
