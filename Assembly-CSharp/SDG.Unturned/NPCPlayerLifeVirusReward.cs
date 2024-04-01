namespace SDG.Unturned;

public class NPCPlayerLifeVirusReward : INPCReward
{
    public int value { get; protected set; }

    public override void GrantReward(Player player)
    {
        player.life.serverModifyVirus(value);
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.read("Reward_Virus");
        }
        return Local.FormatText(text, value);
    }

    public NPCPlayerLifeVirusReward(int newValue, string newText)
        : base(newText)
    {
        value = newValue;
    }
}
