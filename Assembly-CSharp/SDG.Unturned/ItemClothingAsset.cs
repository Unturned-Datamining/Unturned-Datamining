namespace SDG.Unturned;

public class ItemClothingAsset : ItemAsset
{
    protected float _armor;

    protected float _explosionArmor;

    private bool _proofWater;

    private bool _proofFire;

    private bool _proofRadiation;

    internal bool shouldMirrorLeftHandedModel;

    public float movementSpeedMultiplier = 1f;

    public AudioReference wearAudio;

    public float armor => _armor;

    public float explosionArmor => _explosionArmor;

    public override bool showQuality => true;

    public bool proofWater => _proofWater;

    public bool proofFire => _proofFire;

    public bool proofRadiation => _proofRadiation;

    public bool visibleOnRagdoll { get; protected set; }

    public bool hairVisible { get; protected set; }

    public bool beardVisible { get; protected set; }

    public bool shouldDestroyClothingColliders { get; protected set; }

    public string skinOverride { get; protected set; }

    public bool shouldBeVisible(bool isRagdoll)
    {
        if (isRagdoll)
        {
            return visibleOnRagdoll;
        }
        return true;
    }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (builder.shouldRestrictToLegacyContent)
        {
            return;
        }
        if (type == EItemType.HAT || type == EItemType.SHIRT || type == EItemType.PANTS || type == EItemType.VEST)
        {
            if (_armor != 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Clothing_Armor", PlayerDashboardInventoryUI.FormatStatModifier(_armor, higherIsPositive: false, higherIsBeneficial: false)), 10000);
            }
            if (_explosionArmor != 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Clothing_ExplosionArmor", PlayerDashboardInventoryUI.FormatStatModifier(_explosionArmor, higherIsPositive: false, higherIsBeneficial: false)), 10000);
            }
        }
        if (movementSpeedMultiplier != 1f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ClothingMovementSpeedModifier", PlayerDashboardInventoryUI.FormatStatModifier(movementSpeedMultiplier, higherIsPositive: true, higherIsBeneficial: true)), 10000);
        }
        if (_proofFire)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Clothing_FireProof"), 10000);
        }
        if (_proofRadiation)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Clothing_RadiationProof"), 10000);
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        if (isPro)
        {
            _armor = 1f;
            _explosionArmor = 1f;
        }
        else
        {
            _armor = data.ParseFloat("Armor");
            if (data.ContainsKey("Armor"))
            {
                _armor = data.ParseFloat("Armor");
            }
            else
            {
                _armor = 1f;
            }
            if (data.ContainsKey("Armor_Explosion"))
            {
                _explosionArmor = data.ParseFloat("Armor_Explosion");
            }
            else
            {
                _explosionArmor = armor;
            }
            _proofWater = data.ContainsKey("Proof_Water");
            _proofFire = data.ContainsKey("Proof_Fire");
            _proofRadiation = data.ContainsKey("Proof_Radiation");
            movementSpeedMultiplier = data.ParseFloat("Movement_Speed_Multiplier", 1f);
        }
        visibleOnRagdoll = data.ParseBool("Visible_On_Ragdoll", defaultValue: true);
        hairVisible = data.ParseBool("Hair_Visible", defaultValue: true);
        beardVisible = data.ParseBool("Beard_Visible", defaultValue: true);
        shouldMirrorLeftHandedModel = data.ParseBool("Mirror_Left_Handed_Model", defaultValue: true);
        if (data.ContainsKey("WearAudio"))
        {
            wearAudio = data.ReadAudioReference("WearAudio", bundle);
        }
        else if (type == EItemType.BACKPACK || type == EItemType.VEST)
        {
            wearAudio = new AudioReference("core.masterbundle", "Sounds/Zipper.mp3");
        }
        else
        {
            wearAudio = new AudioReference("core.masterbundle", "Sounds/Sleeve.mp3");
        }
        shouldDestroyClothingColliders = data.ParseBool("Destroy_Clothing_Colliders", defaultValue: true);
        skinOverride = data.GetString("Skin_Override");
    }

    protected override AudioReference GetDefaultInventoryAudio()
    {
        if (size_x <= 1 || size_y <= 1)
        {
            return new AudioReference("core.masterbundle", "Sounds/Inventory/LightCloth.asset");
        }
        if (rarity == EItemRarity.COMMON || rarity == EItemRarity.UNCOMMON)
        {
            return new AudioReference("core.masterbundle", "Sounds/Inventory/LightClothEquipment.asset");
        }
        return new AudioReference("core.masterbundle", "Sounds/Inventory/MediumClothEquipment.asset");
    }
}
