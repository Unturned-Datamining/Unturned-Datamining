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

    /// <summary>
    /// Experience to add or subtract when used. Defaults to zero.
    /// </summary>
    public int experience;

    private bool _hasAid;

    private Guid _explosionEffectGuid;

    protected ushort _explosion;

    protected NPCRewardsList questRewardsList;

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

    public INPCReward[] questRewards => questRewardsList.rewards;

    public SpawnTableReward itemRewards { get; protected set; }

    /// <summary>
    /// Canned beans have skins from April Fools.
    /// </summary>
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

    public void GrantQuestRewards(Player player)
    {
        questRewardsList.Grant(player);
    }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (!builder.shouldRestrictToLegacyContent)
        {
            if (_health > 0)
            {
                string arg = PlayerDashboardInventoryUI.FormatStatColor(_health.ToString(), isBeneficial: true);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_HealthPositive", arg), 10000);
            }
            if (_food > 0)
            {
                string arg2 = PlayerDashboardInventoryUI.FormatStatColor(_food.ToString(), isBeneficial: true);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_FoodPositive", arg2), 10000);
            }
            if (_water > 0)
            {
                string arg3 = PlayerDashboardInventoryUI.FormatStatColor(water.ToString(), isBeneficial: true);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_WaterPositive", arg3), 10000);
            }
            if (_virus > 0)
            {
                string arg4 = PlayerDashboardInventoryUI.FormatStatColor(virus.ToString(), isBeneficial: false);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_VirusNegative", arg4), 10000);
            }
            if (_disinfectant > 0)
            {
                string arg5 = PlayerDashboardInventoryUI.FormatStatColor(_disinfectant.ToString(), isBeneficial: true);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_VirusPositive", arg5), 10000);
            }
            if (_energy > 0)
            {
                string arg6 = PlayerDashboardInventoryUI.FormatStatColor(_energy.ToString(), isBeneficial: true);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_StaminaPositive", arg6), 10000);
            }
            if (oxygen > 0)
            {
                string arg7 = PlayerDashboardInventoryUI.FormatStatColor(oxygen.ToString(), isBeneficial: true);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_StaminaPositive", arg7), 10000);
            }
            else if (oxygen < 0)
            {
                string arg8 = PlayerDashboardInventoryUI.FormatStatColor((-oxygen).ToString(), isBeneficial: false);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_OxygenNegative", arg8), 10000);
            }
            int num = Mathf.RoundToInt((float)_warmth / 12.5f);
            if (num > 0)
            {
                string arg9 = PlayerDashboardInventoryUI.FormatStatColor($"{num} s", isBeneficial: true);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_WarmthPositive", arg9), 10000);
            }
            if (itemInstance.quality < 50 && _food + _water > 0)
            {
                builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_ConsumeableMoldy"), isBeneficial: false), 10000);
            }
            switch (bleedingModifier)
            {
            case Bleeding.Heal:
                builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_ConsumeableBleeding_Heal"), isBeneficial: true), 10000);
                break;
            case Bleeding.Cut:
                builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_ConsumeableBleeding_Cut"), isBeneficial: false), 10000);
                break;
            }
            switch (bonesModifier)
            {
            case Bones.Heal:
                builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_ConsumeableBones_Heal"), isBeneficial: true), 10000);
                break;
            case Bones.Break:
                builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_ConsumeableBones_Break"), isBeneficial: false), 10000);
                break;
            }
            if (IsExplosive)
            {
                builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_Explosive"), isBeneficial: false), 2000);
                BuildExplosiveDescription(builder, itemInstance);
            }
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _use = LoadRedirectableAsset<AudioClip>(bundle, "Use", data, "ConsumeAudioClip");
        _health = data.ParseUInt8("Health", 0);
        _food = data.ParseUInt8("Food", 0);
        _water = data.ParseUInt8("Water", 0);
        _virus = data.ParseUInt8("Virus", 0);
        _disinfectant = data.ParseUInt8("Disinfectant", 0);
        _energy = data.ParseUInt8("Energy", 0);
        _vision = data.ParseUInt8("Vision", 0);
        oxygen = data.ParseInt8("Oxygen", 0);
        _warmth = data.ParseUInt32("Warmth");
        experience = data.ParseInt32("Experience");
        if (data.ContainsKey("Bleeding"))
        {
            bleedingModifier = Bleeding.Heal;
        }
        else
        {
            bleedingModifier = data.ParseEnum("Bleeding_Modifier", Bleeding.None);
        }
        if (data.ContainsKey("Broken"))
        {
            bonesModifier = Bones.Heal;
        }
        else
        {
            bonesModifier = data.ParseEnum("Bones_Modifier", Bones.None);
        }
        _hasAid = data.ContainsKey("Aid");
        foodConstrainsWater = food >= water;
        shouldDeleteAfterUse = data.ParseBool("Should_Delete_After_Use", defaultValue: true);
        questRewardsList.Parse(data, localization, this, "Quest_Rewards", "Quest_Reward_");
        ushort tableID = data.ParseUInt16("Item_Reward_Spawn_ID", 0);
        int min = data.ParseInt32("Min_Item_Rewards");
        int max = data.ParseInt32("Max_Item_Rewards");
        itemRewards = new SpawnTableReward(tableID, min, max);
        _explosion = data.ParseGuidOrLegacyId("Explosion", out _explosionEffectGuid);
        IsExplosive = !IsExplosionEffectRefNull();
    }

    [Obsolete("Removed shouldSend parameter")]
    public void grantQuestRewards(Player player, bool shouldSend)
    {
        GrantQuestRewards(player);
    }
}
