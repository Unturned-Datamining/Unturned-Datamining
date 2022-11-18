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

    public bool shouldBeVisible(bool isRagdoll)
    {
        if (isRagdoll)
        {
            return visibleOnRagdoll;
        }
        return true;
    }

    public ItemClothingAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        if (isPro)
        {
            _armor = 1f;
            _explosionArmor = 1f;
        }
        else
        {
            _armor = data.readSingle("Armor");
            if (data.has("Armor"))
            {
                _armor = data.readSingle("Armor");
            }
            else
            {
                _armor = 1f;
            }
            if (data.has("Armor_Explosion"))
            {
                _explosionArmor = data.readSingle("Armor_Explosion");
            }
            else
            {
                _explosionArmor = armor;
            }
            _proofWater = data.has("Proof_Water");
            _proofFire = data.has("Proof_Fire");
            _proofRadiation = data.has("Proof_Radiation");
            movementSpeedMultiplier = data.readSingle("Movement_Speed_Multiplier", 1f);
        }
        visibleOnRagdoll = data.readBoolean("Visible_On_Ragdoll", defaultValue: true);
        hairVisible = data.readBoolean("Hair_Visible", defaultValue: true);
        beardVisible = data.readBoolean("Beard_Visible", defaultValue: true);
        shouldMirrorLeftHandedModel = data.readBoolean("Mirror_Left_Handed_Model", defaultValue: true);
        if (data.has("WearAudio"))
        {
            wearAudio = data.ReadAudioReference("WearAudio");
        }
        else if (type == EItemType.BACKPACK || type == EItemType.VEST)
        {
            wearAudio = new AudioReference("core.masterbundle", "Sounds/Zipper.mp3");
        }
        else
        {
            wearAudio = new AudioReference("core.masterbundle", "Sounds/Sleeve.mp3");
        }
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
