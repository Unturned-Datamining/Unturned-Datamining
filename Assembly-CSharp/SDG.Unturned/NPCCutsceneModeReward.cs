namespace SDG.Unturned;

public class NPCCutsceneModeReward : INPCReward
{
    private bool value;

    public override void GrantReward(Player player)
    {
        player.quests.ServerSetCutsceneModeActive(value);
    }

    public NPCCutsceneModeReward(bool newValue, string newText)
        : base(newText)
    {
        value = newValue;
    }
}
