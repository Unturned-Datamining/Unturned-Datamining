using UnityEngine;

namespace SDG.Unturned;

public class ItemMeleeAsset : ItemWeaponAsset
{
    protected AudioClip _use;

    private float _strength;

    private float _weak;

    private float _strong;

    private byte _stamina;

    private bool _isRepair;

    private bool _isRepeated;

    private bool _isLight;

    public AudioReference impactAudio;

    internal NPCRewardsList weakAttackQuestRewards;

    internal NPCRewardsList strongAttackQuestRewards;

    public AudioClip use => _use;

    public float strength => _strength;

    public float weak => _weak;

    public float strong => _strong;

    public byte stamina => _stamina;

    public bool isRepair => _isRepair;

    public bool isRepeated => _isRepeated;

    public bool isLight => _isLight;

    public PlayerSpotLightConfig lightConfig { get; protected set; }

    public override bool showQuality => true;

    public override bool shouldFriendlySentryTargetUser => true;

    public float alertRadius { get; protected set; }

    protected override bool doesItemTypeHaveSkins => true;

    public override byte[] getState(EItemOrigin origin)
    {
        if (isLight)
        {
            return new byte[1] { 1 };
        }
        return new byte[0];
    }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (builder.shouldRestrictToLegacyContent)
        {
            return;
        }
        if (!_isRepeated)
        {
            if (strength != 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Melee_StrongAttackModifier", PlayerDashboardInventoryUI.FormatStatModifier(strength, higherIsPositive: true, higherIsBeneficial: true)), 10000 + DescSort_HigherIsBeneficial(strength));
            }
            if (stamina > 0)
            {
                string arg = PlayerDashboardInventoryUI.FormatStatColor(stamina.ToString(), isBeneficial: false);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Melee_StrongAttackStamina", arg), 10000);
            }
        }
        BuildNonExplosiveDescription(builder, itemInstance);
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _use = LoadRedirectableAsset<AudioClip>(p.bundle, "Use", p.data, "AttackAudioClip");
        _strength = p.data.ParseFloat("Strength");
        _weak = p.data.ParseFloat("Weak", 0.5f);
        _strong = p.data.ParseFloat("Strong", 0.33f);
        _stamina = p.data.ParseUInt8("Stamina", 0);
        _isRepair = p.data.ContainsKey("Repair");
        _isRepeated = p.data.ContainsKey("Repeated");
        _isLight = p.data.ContainsKey("Light");
        if (isLight)
        {
            lightConfig = new PlayerSpotLightConfig(p.data);
        }
        if (p.data.ContainsKey("Alert_Radius"))
        {
            alertRadius = p.data.ParseFloat("Alert_Radius");
        }
        else
        {
            alertRadius = 8f;
        }
        impactAudio = p.data.ReadAudioReference("ImpactAudioDef", p.bundle);
        weakAttackQuestRewards.Parse(p.data, p.localization, this, "Weak_Attack_Quest_Rewards", "Weak_Attack_Quest_Reward_");
        strongAttackQuestRewards.Parse(p.data, p.localization, this, "Strong_Attack_Quest_Rewards", "Strong_Attack_Quest_Reward_");
    }
}
