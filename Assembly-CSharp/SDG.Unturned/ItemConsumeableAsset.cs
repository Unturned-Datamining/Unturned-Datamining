using System;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class ItemConsumeableAsset : ItemWeaponAsset
{
    public enum Bleeding
    {
        None,
        Heal,
        Cut
    }

    public enum Bones
    {
        None,
        Heal,
        Break
    }

    protected AudioClip _use;

    private byte _health;

    private byte _food;

    private byte _water;

    private byte _virus;

    private byte _disinfectant;

    private byte _energy;

    private byte _vision;

    private uint _warmth;

    public int experience;

    private bool _hasAid;

    private Guid _explosionEffectGuid;

    protected ushort _explosion;

    public AudioClip use => _use;

    public byte health => _health;

    public byte food => _food;

    public byte water => _water;

    public byte virus => _virus;

    public byte disinfectant => _disinfectant;

    public byte energy => _energy;

    public byte vision => _vision;

    public sbyte oxygen { get; protected set; }

    public uint warmth => _warmth;

    public Bleeding bleedingModifier { get; protected set; }

    public Bones bonesModifier { get; protected set; }

    public bool hasAid => _hasAid;

    public bool foodConstrainsWater { get; protected set; }

    public bool shouldDeleteAfterUse { get; protected set; }

    public override bool showQuality
    {
        get
        {
            if (type != EItemType.FOOD)
            {
                return type == EItemType.WATER;
            }
            return true;
        }
    }

    public Guid ExplosionEffectGuid => _explosionEffectGuid;

    public ushort explosion
    {
        [Obsolete]
        get
        {
            return _explosion;
        }
    }

    public bool IsExplosive { get; private set; }

    public INPCReward[] questRewards { get; protected set; }

    public SpawnTableReward itemRewards { get; protected set; }

    protected override bool doesItemTypeHaveSkins => id == 13;

    public override bool shouldFriendlySentryTargetUser
    {
        get
        {
            if (IsExplosive)
            {
                return true;
            }
            return base.shouldFriendlySentryTargetUser;
        }
    }

    public bool IsExplosionEffectRefNull()
    {
        if (_explosion == 0)
        {
            return _explosionEffectGuid.IsEmpty();
        }
        return false;
    }

    public EffectAsset FindExplosionEffectAsset()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(_explosionEffectGuid, _explosion);
    }

    public void grantQuestRewards(Player player, bool shouldSend)
    {
        if (questRewards != null)
        {
            for (int i = 0; i < questRewards.Length; i++)
            {
                questRewards[i].grantReward(player, shouldSend);
            }
        }
    }

    public ItemConsumeableAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _use = LoadRedirectableAsset<AudioClip>(bundle, "Use", data, "ConsumeAudioClip");
        _health = data.readByte("Health", 0);
        _food = data.readByte("Food", 0);
        _water = data.readByte("Water", 0);
        _virus = data.readByte("Virus", 0);
        _disinfectant = data.readByte("Disinfectant", 0);
        _energy = data.readByte("Energy", 0);
        _vision = data.readByte("Vision", 0);
        oxygen = data.readSByte("Oxygen", 0);
        _warmth = data.readUInt32("Warmth");
        experience = data.readInt32("Experience");
        if (data.has("Bleeding"))
        {
            bleedingModifier = Bleeding.Heal;
        }
        else
        {
            bleedingModifier = data.readEnum("Bleeding_Modifier", Bleeding.None);
        }
        if (data.has("Broken"))
        {
            bonesModifier = Bones.Heal;
        }
        else
        {
            bonesModifier = data.readEnum("Bones_Modifier", Bones.None);
        }
        _hasAid = data.has("Aid");
        foodConstrainsWater = food >= water;
        shouldDeleteAfterUse = data.readBoolean("Should_Delete_After_Use", defaultValue: true);
        questRewards = new INPCReward[data.readByte("Quest_Rewards", 0)];
        NPCTool.readRewards(data, localization, "Quest_Reward_", questRewards, this);
        ushort tableID = data.readUInt16("Item_Reward_Spawn_ID", 0);
        int min = data.readInt32("Min_Item_Rewards");
        int max = data.readInt32("Max_Item_Rewards");
        itemRewards = new SpawnTableReward(tableID, min, max);
        _explosion = data.ReadGuidOrLegacyId("Explosion", out _explosionEffectGuid);
        IsExplosive = !IsExplosionEffectRefNull();
    }
}
