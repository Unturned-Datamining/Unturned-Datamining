namespace SDG.Unturned;

public class NPCHintReward : INPCReward
{
    private float duration;

    public override void grantReward(Player player, bool shouldSend)
    {
        if (shouldSend)
        {
            if (Provider.isServer)
            {
                player.ServerShowHint(text, duration);
            }
        }
        else if (Player.player == player)
        {
            PlayerUI.message(EPlayerMessage.NPC_CUSTOM, text, duration);
        }
    }

    public NPCHintReward(float newDuration, string newText)
        : base(newText)
    {
        duration = newDuration;
    }
}
