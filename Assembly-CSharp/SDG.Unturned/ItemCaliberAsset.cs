namespace SDG.Unturned;

public class ItemCaliberAsset : ItemAsset
{
    private ushort[] _calibers;

    private float _recoil_x;

    private float _recoil_y;

    public float aimingRecoilMultiplier;

    public float aimDurationMultiplier;

    private float _spread;

    private float _sway;

    private float _shake;

    private float _damage;

    private byte _firerate;

    protected bool _isPaintable;

    public float aimingMovementSpeedMultiplier;

    protected bool _isBipod;

    public ushort[] calibers => _calibers;

    public float recoil_x => _recoil_x;

    public float recoil_y => _recoil_y;

    public float spread => _spread;

    public float sway => _sway;

    public float shake => _shake;

    public float damage => _damage;

    public byte firerate => _firerate;

    public bool isPaintable => _isPaintable;

    public float ballisticDamageMultiplier { get; protected set; }

    public bool ShouldOnlyAffectAimWhileProne => _isBipod;

    public bool shouldDestroyAttachmentColliders { get; protected set; }

    public string instantiatedAttachmentName { get; protected set; }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _calibers = new ushort[data.ParseUInt8("Calibers", 0)];
        for (byte b = 0; b < calibers.Length; b = (byte)(b + 1))
        {
            _calibers[b] = data.ParseUInt16("Caliber_" + b, 0);
        }
        _recoil_x = data.ParseFloat("Recoil_X", 1f);
        _recoil_y = data.ParseFloat("Recoil_Y", 1f);
        aimingRecoilMultiplier = data.ParseFloat("Aiming_Recoil_Multiplier", 1f);
        aimDurationMultiplier = data.ParseFloat("Aim_Duration_Multiplier", 1f);
        _spread = data.ParseFloat("Spread", 1f);
        _sway = data.ParseFloat("Sway", 1f);
        _shake = data.ParseFloat("Shake", 1f);
        _damage = data.ParseFloat("Damage", 1f);
        _firerate = data.ParseUInt8("Firerate", 0);
        float defaultValue = data.ParseFloat("Damage", 1f);
        ballisticDamageMultiplier = data.ParseFloat("Ballistic_Damage_Multiplier", defaultValue);
        aimingMovementSpeedMultiplier = data.ParseFloat("Aiming_Movement_Speed_Multiplier", 1f);
        _isPaintable = data.ContainsKey("Paintable");
        _isBipod = data.ContainsKey("Bipod");
        shouldDestroyAttachmentColliders = data.ParseBool("Destroy_Attachment_Colliders", defaultValue: true);
        instantiatedAttachmentName = data.GetString("Instantiated_Attachment_Name_Override", GUID.ToString("N"));
    }

    protected override AudioReference GetDefaultInventoryAudio()
    {
        return new AudioReference("core.masterbundle", "Sounds/Inventory/SmallGunAttachment.asset");
    }
}
