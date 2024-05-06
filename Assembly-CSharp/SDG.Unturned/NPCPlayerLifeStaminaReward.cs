namespace SDG.Unturned;

public class NPCPlayerLifeStaminaReward : INPCReward
{
    public int value { get; protected set; }

    public override void GrantReward(Player player)
    {
        player.life.serverModifyStamina(value);
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.read("Reward_Stamina");
        }
        return Local.FormatText(text, value);
    }

    public NPCPlayerLifeStaminaReward(int newValue, string newText)
        : base(newText)
    {
        value = newValue;
    }
}
