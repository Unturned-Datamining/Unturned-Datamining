namespace SDG.Unturned;

public class NPCReputationReward : INPCReward
{
    public int value { get; protected set; }

    public override void grantReward(Player player, bool shouldSend)
    {
        if (shouldSend)
        {
            player.skills.askRep(value);
        }
        else
        {
            player.skills.modRep(value);
        }
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
