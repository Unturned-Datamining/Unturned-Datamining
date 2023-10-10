namespace SDG.Unturned;

public class NPCPlayerLifeHealthReward : INPCReward
{
    public int value { get; protected set; }

    public override void GrantReward(Player player)
    {
        player.life.serverModifyHealth(value);
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.read("Reward_Health");
        }
        return string.Format(text, value);
    }

    public NPCPlayerLifeHealthReward(int newValue, string newText)
        : base(newText)
    {
        value = newValue;
    }
}
