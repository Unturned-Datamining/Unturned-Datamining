namespace SDG.Unturned;

public class NPCExperienceReward : INPCReward
{
    public uint value { get; protected set; }

    public override void GrantReward(Player player)
    {
        player.skills.askAward(value);
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
