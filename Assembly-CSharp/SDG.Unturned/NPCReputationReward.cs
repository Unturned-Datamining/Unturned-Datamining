namespace SDG.Unturned;

public class NPCReputationReward : INPCReward
{
    public int value { get; protected set; }

    public override void GrantReward(Player player)
    {
        player.skills.askRep(value);
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(base.text))
        {
            base.text = PlayerNPCQuestUI.localization.read("Reward_Reputation");
        }
        string text = value.ToString();
        if (value > 0)
        {
            text = "+" + text;
        }
        return string.Format(base.text, text);
    }

    public NPCReputationReward(int newValue, string newText)
        : base(newText)
    {
        value = newValue;
    }
}
