using UnityEngine;

namespace SDG.Unturned;

public class ItemBarrelAsset : ItemCaliberAsset
{
    protected AudioClip _shoot;

    protected GameObject _barrel;

    private bool _isBraked;

    private bool _isSilenced;

    private float _volume;

    private byte _durability;

    private float _ballisticDrop;

    public AudioClip shoot => _shoot;

    public GameObject barrel => _barrel;

    public bool isBraked => _isBraked;

    public bool isSilenced => _isSilenced;

    public float volume => _volume;

    public byte durability => _durability;

    public override bool showQuality => durability > 0;

    public float ballisticDrop => _ballisticDrop;

    public float gunshotRolloffDistanceMultiplier { get; protected set; }

    public ItemBarrelAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _shoot = bundle.load<AudioClip>("Shoot");
        _barrel = loadRequiredAsset<GameObject>(bundle, "Barrel");
        _isBraked = data.has("Braked");
        _isSilenced = data.has("Silenced");
        _volume = data.readSingle("Volume", 1f);
        _durability = data.readByte("Durability", 0);
        if (data.has("Ballistic_Drop"))
        {
            _ballisticDrop = data.readSingle("Ballistic_Drop");
        }
        else
        {
            _ballisticDrop = 1f;
        }
        float defaultValue = (isSilenced ? 0.5f : 1f);
        gunshotRolloffDistanceMultiplier = data.readSingle("Gunshot_Rolloff_Distance_Multiplier", defaultValue);
    }
}
