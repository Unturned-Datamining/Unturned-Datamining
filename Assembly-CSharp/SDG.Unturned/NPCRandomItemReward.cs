namespace SDG.Unturned;

public class NPCRandomItemReward : INPCReward
{
    public ushort id { get; protected set; }

    public byte amount { get; protected set; }

    public bool shouldAutoEquip { get; protected set; }

    public EItemOrigin origin { get; protected set; }

    public override void GrantReward(Player player)
    {
        for (byte b = 0; b < amount; b++)
        {
            ushort num = SpawnTableTool.ResolveLegacyId(id, EAssetType.ITEM, OnGetSpawnTableErrorContext);
            if (num != 0)
            {
                player.inventory.forceAddItem(new Item(num, origin), shouldAutoEquip, playEffect: false);
            }
        }
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.read("Reward_Item_Random");
        }
        return Local.FormatText(text, amount);
    }

    public NPCRandomItemReward(ushort newID, byte newAmount, bool newShouldAutoEquip, EItemOrigin origin, string newText)
        : base(newText)
    {
        id = newID;
        amount = newAmount;
        shouldAutoEquip = newShouldAutoEquip;
        this.origin = origin;
    }

    private string OnGetSpawnTableErrorContext()
    {
        return "NPC random item reward";
    }
}
