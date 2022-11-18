namespace SDG.Unturned;

public class NPCExperienceReward : INPCReward
{
    public uint value { get; protected set; }

    public override void grantReward(Player player, bool shouldSend)
    {
        if (shouldSend)
        {
            player.skills.askAward(value);
        }
        else
        {
            player.skills.modXp(value);
        }
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.read("Reward_Experience");
        }
        string arg = "+" + value;
        return string.Format(text, arg);
    }

    public NPCExperienceReward(uint newValue, string newText)
        : base(newText)
    {
        value = newValue;
    }
}
