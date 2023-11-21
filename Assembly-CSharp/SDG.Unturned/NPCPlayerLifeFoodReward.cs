namespace SDG.Unturned;

public class NPCPlayerLifeFoodReward : INPCReward
{
    public int value { get; protected set; }

    public override void GrantReward(Player player)
    {
        player.life.serverModifyFood(value);
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.read("Reward_Food");
        }
        return string.Format(text, value);
    }

    public NPCPlayerLifeFoodReward(int newValue, string newText)
        : base(newText)
    {
        value = newValue;
    }
}
