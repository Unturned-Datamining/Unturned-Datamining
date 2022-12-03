namespace SDG.Unturned;

public class NPCRandomItemReward : INPCReward
{
    public ushort id { get; protected set; }

    public byte amount { get; protected set; }

    public bool shouldAutoEquip { get; protected set; }

    public override void grantReward(Player player, bool shouldSend)
    {
        if (!Provider.isServer)
        {
            return;
        }
        for (byte b = 0; b < amount; b = (byte)(b + 1))
        {
            ushort num = SpawnTableTool.resolve(id);
            if (num != 0)
            {
                player.inventory.forceAddItem(new Item(num, EItemOrigin.CRAFT), shouldAutoEquip, playEffect: false);
            }
        }
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.read("Reward_Item_Random");
        }
        return string.Format(text, amount);
    }

    public NPCRandomItemReward(ushort newID, byte newAmount, bool newShouldAutoEquip, string newText)
        : base(newText)
    {
        id = newID;
        amount = newAmount;
        shouldAutoEquip = newShouldAutoEquip;
    }
}
