using System;

namespace SDG.Unturned;

public class NPCRandomItemReward : INPCReward
{
    private CachingBcAssetRef _spawnAssetRef;

    public CachingBcAssetRef SpawnAssetRef => _spawnAssetRef;

    public byte amount { get; protected set; }

    public bool shouldAutoEquip { get; protected set; }

    public EItemOrigin origin { get; protected set; }

    [Obsolete]
    public Guid SpawnTableGuid => _spawnAssetRef.Guid;

    [Obsolete]
    public ushort id => _spawnAssetRef.LegacyId;

    public SpawnAsset FindSpawnAsset()
    {
        return _spawnAssetRef.Get<SpawnAsset>();
    }

    public override void GrantReward(Player player)
    {
        SpawnAsset spawnAsset = FindSpawnAsset();
        if (spawnAsset == null)
        {
            return;
        }
        for (byte b = 0; b < amount; b++)
        {
            ushort num = SpawnTableTool.ResolveLegacyId(spawnAsset, EAssetType.ITEM, OnGetSpawnTableErrorContext);
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
            text = PlayerNPCQuestUI.localization.FormatOrEmpty("Reward_Item_Random");
        }
        return Local.FormatText(text, amount);
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (!p.data.TryParseBcAssetRef("ID", EAssetType.SPAWN, out _spawnAssetRef))
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        if (p.data.TryParseUInt8("Amount", out var value))
        {
            amount = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Amount");
        }
        shouldAutoEquip = p.data.ParseBool("Auto_Equip");
        origin = p.data.ParseEnum("Origin", EItemOrigin.CRAFT);
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        if (!p.data.TryParseBcAssetRef(p.legacyPrefix + "_ID", EAssetType.SPAWN, out _spawnAssetRef))
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        if (p.data.TryParseUInt8(p.legacyPrefix + "_Amount", out var value))
        {
            amount = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Amount");
        }
        shouldAutoEquip = p.data.ParseBool(p.legacyPrefix + "_Auto_Equip");
        origin = p.data.ParseEnum(p.legacyPrefix + "_Origin", EItemOrigin.CRAFT);
    }

    public NPCRandomItemReward()
    {
    }

    [Obsolete]
    public NPCRandomItemReward(ushort newID, byte newAmount, bool newShouldAutoEquip, EItemOrigin origin, string newText)
        : base(newText)
    {
        _spawnAssetRef = new CachingBcAssetRef(EAssetType.SPAWN, newID);
        amount = newAmount;
        shouldAutoEquip = newShouldAutoEquip;
        this.origin = origin;
    }

    private string OnGetSpawnTableErrorContext()
    {
        return "NPC random item reward";
    }
}
