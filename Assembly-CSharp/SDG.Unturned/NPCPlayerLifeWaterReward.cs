namespace SDG.Unturned;

public class NPCPlayerLifeWaterReward : INPCReward
{
    public int value { get; protected set; }

    public override void GrantReward(Player player)
    {
        player.life.serverModifyWater(value);
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.read("Reward_Water");
        }
        return string.Format(text, value);
    }

    public NPCPlayerLifeWaterReward(int newValue, string newText)
        : base(newText)
    {
        value = newValue;
    }
}
