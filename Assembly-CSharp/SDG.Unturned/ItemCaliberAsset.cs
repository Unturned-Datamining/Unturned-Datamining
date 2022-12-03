namespace SDG.Unturned;

public class ItemCaliberAsset : ItemAsset
{
    private ushort[] _calibers;

    private float _recoil_x;

    private float _recoil_y;

    public float aimingRecoilMultiplier;

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

    public ItemCaliberAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _calibers = new ushort[data.readByte("Calibers", 0)];
        for (byte b = 0; b < calibers.Length; b = (byte)(b + 1))
        {
            _calibers[b] = data.readUInt16("Caliber_" + b, 0);
        }
        _recoil_x = data.readSingle("Recoil_X", 1f);
        _recoil_y = data.readSingle("Recoil_Y", 1f);
        aimingRecoilMultiplier = data.readSingle("Aiming_Recoil_Multiplier", 1f);
        _spread = data.readSingle("Spread", 1f);
        _sway = data.readSingle("Sway", 1f);
        _shake = data.readSingle("Shake", 1f);
        _damage = data.readSingle("Damage", 1f);
        _firerate = data.readByte("Firerate", 0);
        float defaultValue = data.readSingle("Damage", 1f);
        ballisticDamageMultiplier = data.readSingle("Ballistic_Damage_Multiplier", defaultValue);
        aimingMovementSpeedMultiplier = data.readSingle("Aiming_Movement_Speed_Multiplier", 1f);
        _isPaintable = data.has("Paintable");
        _isBipod = data.has("Bipod");
    }

    protected override AudioReference GetDefaultInventoryAudio()
    {
        return new AudioReference("core.masterbundle", "Sounds/Inventory/SmallGunAttachment.asset");
    }
}
