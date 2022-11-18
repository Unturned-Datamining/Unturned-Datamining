using System;
using UnityEngine;

namespace SDG.Unturned;

public class EffectAsset : Asset
{
    protected GameObject _effect;

    protected GameObject[] _splatters;

    private bool _gore;

    private byte _splatter;

    private float _splatterLifetime;

    private float _splatterLifetimeSpread;

    private bool _splatterLiquid;

    private EPlayerTemperature _splatterTemperature;

    private byte _splatterPreload;

    private float _lifetime;

    private float _lifetimeSpread;

    private bool _isStatic;

    private byte _preload;

    public Guid blastmarkEffectGuid;

    private ushort _blast;

    public float cameraShakeRadius;

    public float cameraShakeMagnitudeDegrees;

    public GameObject effect => _effect;

    public GameObject[] splatters => _splatters;

    public bool gore => _gore;

    public byte splatter => _splatter;

    public float splatterLifetime => _splatterLifetime;

    public float splatterLifetimeSpread => _splatterLifetimeSpread;

    public bool splatterLiquid => _splatterLiquid;

    public EPlayerTemperature splatterTemperature => _splatterTemperature;

    public byte splatterPreload => _splatterPreload;

    public float lifetime => _lifetime;

    public float lifetimeSpread => _lifetimeSpread;

    public bool isStatic => _isStatic;

    public byte preload => _preload;

    public ushort blast
    {
        [Obsolete]
        get
        {
            return _blast;
        }
    }

    public float relevantDistance { get; protected set; }

    public bool spawnOnDedicatedServer { get; protected set; }

    public bool randomizeRotation { get; protected set; }

    public override EAssetType assetCategory => EAssetType.EFFECT;

    public EffectAsset FindBlastmarkEffectAsset()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(blastmarkEffectGuid, blast);
    }

    public EffectAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        if (id < 200 && !bundle.hasResource && !data.has("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 200");
        }
        _effect = bundle.load<GameObject>("Effect");
        if (effect == null)
        {
            throw new NotSupportedException("Missing effect gameobject");
        }
        _gore = data.has("Gore");
        _splatters = new GameObject[data.readByte("Splatter", 0)];
        for (int i = 0; i < splatters.Length; i++)
        {
            splatters[i] = bundle.load<GameObject>("Splatter_" + i);
            if (splatters[i] == null)
            {
                Assets.reportError(this, $"missing 'Splatter_{i}' gameobject");
            }
        }
        _splatter = data.readByte("Splatters", 0);
        _splatterLiquid = data.has("Splatter_Liquid");
        if (data.has("Splatter_Temperature"))
        {
            _splatterTemperature = (EPlayerTemperature)Enum.Parse(typeof(EPlayerTemperature), data.readString("Splatter_Temperature"), ignoreCase: true);
        }
        else
        {
            _splatterTemperature = EPlayerTemperature.NONE;
        }
        _splatterLifetime = data.readSingle("Splatter_Lifetime");
        if (data.has("Splatter_Lifetime_Spread"))
        {
            _splatterLifetimeSpread = data.readSingle("Splatter_Lifetime_Spread");
        }
        else
        {
            _splatterLifetimeSpread = 1f;
        }
        _lifetime = data.readSingle("Lifetime");
        if (data.has("Lifetime_Spread"))
        {
            _lifetimeSpread = data.readSingle("Lifetime_Spread");
        }
        else
        {
            _lifetimeSpread = 4f;
        }
        _isStatic = data.has("Static");
        if (data.has("Preload"))
        {
            _preload = data.readByte("Preload", 0);
        }
        else
        {
            _preload = 1;
        }
        if (data.has("Splatter_Preload"))
        {
            _splatterPreload = data.readByte("Splatter_Preload", 0);
        }
        else
        {
            _splatterPreload = (byte)(Mathf.CeilToInt((float)(int)splatter / (float)splatters.Length) * preload);
        }
        _blast = data.ReadGuidOrLegacyId("Blast", out blastmarkEffectGuid);
        relevantDistance = data.readSingle("Relevant_Distance", -1f);
        spawnOnDedicatedServer = data.has("Spawn_On_Dedicated_Server");
        if (data.has("Randomize_Rotation"))
        {
            randomizeRotation = data.readBoolean("Randomize_Rotation");
        }
        else
        {
            randomizeRotation = true;
        }
        cameraShakeRadius = data.readSingle("CameraShake_Radius");
        cameraShakeMagnitudeDegrees = data.readSingle("CameraShake_MagnitudeDegrees");
    }
}
