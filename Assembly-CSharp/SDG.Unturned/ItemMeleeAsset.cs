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

    public ItemMeleeAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _use = LoadRedirectableAsset<AudioClip>(bundle, "Use", data, "AttackAudioClip");
        _strength = data.readSingle("Strength");
        _weak = data.readSingle("Weak");
        if ((double)weak < 0.01)
        {
            _weak = 0.5f;
        }
        _strong = data.readSingle("Strong");
        if ((double)strong < 0.01)
        {
            _strong = 0.33f;
        }
        _stamina = data.readByte("Stamina", 0);
        _isRepair = data.has("Repair");
        _isRepeated = data.has("Repeated");
        _isLight = data.has("Light");
        if (isLight)
        {
            lightConfig = new PlayerSpotLightConfig(data);
        }
        if (data.has("Alert_Radius"))
        {
            alertRadius = data.readSingle("Alert_Radius");
        }
        else
        {
            alertRadius = 8f;
        }
        impactAudio = data.ReadAudioReference("ImpactAudioDef");
    }
}
