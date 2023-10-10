namespace SDG.Unturned;

public class NPCHintReward : INPCReward
{
    private float duration;

    public override void GrantReward(Player player)
    {
        player.ServerShowHint(text, duration);
    }

    public NPCHintReward(float newDuration, string newText)
        : base(newText)
    {
        duration = newDuration;
    }
}
