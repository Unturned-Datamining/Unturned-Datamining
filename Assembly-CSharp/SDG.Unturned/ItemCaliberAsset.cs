using System;

namespace SDG.Unturned;

public class ItemCaliberAsset : ItemAsset
{
    private ushort[] _calibers;

    private float _recoil_x;

    private float _recoil_y;

    /// <summary>
    /// Recoil magnitude multiplier while the gun is aiming down sights.
    /// </summary>
    public float aimingRecoilMultiplier;

    /// <summary>
    /// Multiplier for gun's Aim_In_Duration.
    /// </summary>
    public float aimDurationMultiplier;

    private float _spread;

    private float _sway;

    private float _shake;

    private int _firerateOffset;

    protected bool _isPaintable;

    /// <summary>
    /// Movement speed multiplier while the gun is aiming down sights.
    /// </summary>
    public float aimingMovementSpeedMultiplier;

    protected bool _isBipod;

    public ushort[] calibers => _calibers;

    public float recoil_x => _recoil_x;

    public float recoil_y => _recoil_y;

    public float spread => _spread;

    public float sway => _sway;

    public float shake => _shake;

    /// <summary>
    /// For backwards compatibility this is *subtracted* from the gun's firerate, so a positive number decreases
    /// the time between shots and a negative number increases the time between shots.
    /// </summary>
    public int FirerateOffset => _firerateOffset;

    public bool isPaintable => _isPaintable;

    /// <summary>
    /// Multiplier for normal bullet damage.
    /// </summary>
    public float ballisticDamageMultiplier { get; protected set; }

    /// <summary>
    /// Multiplier for bullet acceleration due to gravity.
    /// </summary>
    public float BallisticGravityMultiplier { get; protected set; }

    public bool ShouldOnlyAffectAimWhileProne => _isBipod;

    /// <summary>
    /// If true, gun can damage entities with Invulnerable tag. Defaults to false.
    /// </summary>
    public bool CanDamageInvulernableEntities { get; protected set; }

    public bool shouldDestroyAttachmentColliders { get; protected set; }

    /// <summary>
    /// Name to use when instantiating attachment prefab.
    /// By default the asset guid is used, but it can be overridden because some
    /// modders rely on the name for Unity's legacy animation component. For example
    /// in Toothy Deerryte's case there were a lot of duplicate animations to work
    /// around the guid naming, simplified by overriding name.
    /// </summary>
    public string instantiatedAttachmentName { get; protected set; }

    protected override bool doesItemTypeHaveSkins => true;

    [Obsolete("Changed type to int")]
    public byte firerate => MathfEx.ClampToByte(_firerateOffset);

    /// <summary>
    /// Returns true if calibers list contains provided caliber ID.
    /// </summary>
    public bool CalibersContainId(ushort caliberId)
    {
        ushort[] array = calibers;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == caliberId)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns true if calibers list contains any of the provided caliber IDs.
    /// </summary>
    public bool CalibersContainAnyOfIds(ushort[] caliberIds)
    {
        foreach (ushort caliberId in caliberIds)
        {
            if (CalibersContainId(caliberId))
            {
                return true;
            }
        }
        return false;
    }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (!builder.shouldRestrictToLegacyContent)
        {
            if (_recoil_x != 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_RecoilModifier_X", PlayerDashboardInventoryUI.FormatStatModifier(_recoil_x, higherIsPositive: false, higherIsBeneficial: false)), 10000 + DescSort_LowerIsBeneficial(_recoil_x));
            }
            if (_recoil_y != 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_RecoilModifier_Y", PlayerDashboardInventoryUI.FormatStatModifier(_recoil_y, higherIsPositive: false, higherIsBeneficial: false)), 10000 + DescSort_LowerIsBeneficial(_recoil_y));
            }
            if (_spread != 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_SpreadModifier", PlayerDashboardInventoryUI.FormatStatModifier(_spread, higherIsPositive: false, higherIsBeneficial: false)), 10000 + DescSort_LowerIsBeneficial(_spread));
            }
            if (_sway != 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_SwayModifier", PlayerDashboardInventoryUI.FormatStatModifier(_sway, higherIsPositive: true, higherIsBeneficial: false)), 10000 + DescSort_LowerIsBeneficial(_sway));
            }
            if (aimingRecoilMultiplier != 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_RecoilModifier_Aiming", PlayerDashboardInventoryUI.FormatStatModifier(aimingRecoilMultiplier, higherIsPositive: false, higherIsBeneficial: false)), 10000 + DescSort_LowerIsBeneficial(aimingRecoilMultiplier));
            }
            if (aimDurationMultiplier != 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_AimDurationModifier", PlayerDashboardInventoryUI.FormatStatModifier(aimDurationMultiplier, higherIsPositive: false, higherIsBeneficial: false)), 10000 + DescSort_LowerIsBeneficial(aimDurationMultiplier));
            }
            if (aimingMovementSpeedMultiplier != 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_AimingMovementSpeedModifier", PlayerDashboardInventoryUI.FormatStatModifier(aimingMovementSpeedMultiplier, higherIsPositive: true, higherIsBeneficial: true)), 10000 + DescSort_HigherIsBeneficial(aimingMovementSpeedMultiplier));
            }
            if (ballisticDamageMultiplier != 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_BulletDamageModifier", PlayerDashboardInventoryUI.FormatStatModifier(ballisticDamageMultiplier, higherIsPositive: true, higherIsBeneficial: true)), 10000 + DescSort_HigherIsBeneficial(ballisticDamageMultiplier));
            }
            if (BallisticGravityMultiplier != 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_BulletGravityModifier", PlayerDashboardInventoryUI.FormatStatModifier(BallisticGravityMultiplier, higherIsPositive: true, higherIsBeneficial: false)), 10000 + DescSort_LowerIsBeneficial(BallisticGravityMultiplier));
            }
            if (CanDamageInvulernableEntities)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_InvulnerableModifier"), 10000);
            }
        }
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _calibers = new ushort[p.data.ParseUInt8("Calibers", 0)];
        for (byte b = 0; b < calibers.Length; b++)
        {
            _calibers[b] = p.data.ParseUInt16("Caliber_" + b, 0);
        }
        _recoil_x = p.data.ParseFloat("Recoil_X", 1f);
        _recoil_y = p.data.ParseFloat("Recoil_Y", 1f);
        aimingRecoilMultiplier = p.data.ParseFloat("Aiming_Recoil_Multiplier", 1f);
        aimDurationMultiplier = p.data.ParseFloat("Aim_Duration_Multiplier", 1f);
        _spread = p.data.ParseFloat("Spread", 1f);
        _sway = p.data.ParseFloat("Sway", 1f);
        _shake = p.data.ParseFloat("Shake", 1f);
        _firerateOffset = p.data.ParseInt32("Firerate");
        float defaultValue = p.data.ParseFloat("Damage", 1f);
        ballisticDamageMultiplier = p.data.ParseFloat("Ballistic_Damage_Multiplier", defaultValue);
        BallisticGravityMultiplier = p.data.ParseFloat("Ballistic_Drop", 1f);
        aimingMovementSpeedMultiplier = p.data.ParseFloat("Aiming_Movement_Speed_Multiplier", 1f);
        _isPaintable = p.data.ContainsKey("Paintable");
        _isBipod = p.data.ContainsKey("Bipod");
        CanDamageInvulernableEntities = p.data.ParseBool("Invulnerable");
        shouldDestroyAttachmentColliders = p.data.ParseBool("Destroy_Attachment_Colliders", defaultValue: true);
        instantiatedAttachmentName = p.data.GetString("Instantiated_Attachment_Name_Override", GUID.ToString("N"));
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Caliber");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Recoil_X", recoil_x);
        orAddDeclaration.Append("Recoil_Y", recoil_y);
        orAddDeclaration.Append("Aiming_Recoil_Multiplier", aimingRecoilMultiplier);
        orAddDeclaration.Append("Aim_Duration_Multiplier", aimDurationMultiplier);
        orAddDeclaration.Append("Spread", spread);
        orAddDeclaration.Append("Sway", sway);
        orAddDeclaration.Append("Shake", shake);
        orAddDeclaration.Append("Firerate", FirerateOffset);
        orAddDeclaration.Append("Ballistic_Damage_Multiplier", ballisticDamageMultiplier);
        orAddDeclaration.Append("Ballistic_Drop", BallisticGravityMultiplier);
        orAddDeclaration.Append("Aiming_Movement_Speed_Multiplier", aimingMovementSpeedMultiplier);
        orAddDeclaration.Append("Paintable", isPaintable);
        orAddDeclaration.Append("Bipod", ShouldOnlyAffectAimWhileProne);
        orAddDeclaration.Append("Invulnerable", CanDamageInvulernableEntities);
        orAddDeclaration.Append("Calibers", calibers.Length);
        for (byte b = 0; b < calibers.Length; b++)
        {
            CargoDeclaration cargoDeclaration = builder.AddDeclaration("Caliber_Caliber");
            cargoDeclaration.Append("GUID", GUID);
            cargoDeclaration.Append("Caliber", calibers[b]);
        }
    }

    protected override AudioReference GetDefaultInventoryAudio()
    {
        return new AudioReference("core.masterbundle", "Sounds/Inventory/SmallGunAttachment.asset");
    }
}
