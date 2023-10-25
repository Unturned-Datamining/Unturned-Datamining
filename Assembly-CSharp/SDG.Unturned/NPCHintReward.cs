namespace SDG.Unturned;

public class NPCHintReward : INPCReward
{
    /// <summary>
    /// How many seconds message should popup.
    /// </summary>
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
