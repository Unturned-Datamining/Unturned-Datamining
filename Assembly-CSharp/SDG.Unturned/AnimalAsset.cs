using System;
using UnityEngine;

namespace SDG.Unturned;

public class AnimalAsset : Asset
{
    protected string _animalName;

    protected GameObject _client;

    protected GameObject _server;

    protected GameObject _dedicated;

    protected GameObject _ragdoll;

    protected float _speedRun;

    protected float _speedWalk;

    private EAnimalBehaviour _behaviour;

    protected ushort _health;

    protected uint _rewardXP;

    protected float _regen;

    protected byte _damage;

    protected ushort _meat;

    protected ushort _pelt;

    private byte _rewardMin;

    private byte _rewardMax;

    private ushort _rewardID;

    protected AudioClip[] _roars;

    protected AudioClip[] _panics;

    public float attackInterval;

    public string animalName => _animalName;

    public override string FriendlyName => _animalName;

    public GameObject client => _client;

    public GameObject server => _server;

    public GameObject dedicated => _dedicated;

    public GameObject ragdoll => _ragdoll;

    public float speedRun => _speedRun;

    public float speedWalk => _speedWalk;

    public EAnimalBehaviour behaviour => _behaviour;

    public ushort health => _health;

    public uint rewardXP => _rewardXP;

    public float regen => _regen;

    public byte damage => _damage;

    public ushort meat => _meat;

    public ushort pelt => _pelt;

    public byte rewardMin => _rewardMin;

    public byte rewardMax => _rewardMax;

    public ushort rewardID => _rewardID;

    public AudioClip[] roars => _roars;

    public AudioClip[] panics => _panics;

    public int attackAnimVariantsCount { get; protected set; }

    public int eatAnimVariantsCount { get; protected set; }

    public int glanceAnimVariantsCount { get; protected set; }

    public int startleAnimVariantsCount { get; protected set; }

    public float horizontalAttackRangeSquared { get; protected set; }

    public float horizontalVehicleAttackRangeSquared { get; protected set; }

    public float verticalAttackRange { get; protected set; }

    public override EAssetType assetCategory => EAssetType.ANIMAL;

    public bool shouldPlayAnimsOnDedicatedServer { get; private set; }

    protected void validateAnimations(GameObject root)
    {
        Animation animation = root.transform.Find("Character")?.GetComponent<Animation>();
        if (animation == null)
        {
            Assets.reportError(this, "{0} missing Animation component on Character", root);
            return;
        }
        validateAnimation(animation, "Idle");
        validateAnimation(animation, "Walk");
        validateAnimation(animation, "Run");
        if (attackAnimVariantsCount > 1)
        {
            for (int i = 0; i < attackAnimVariantsCount; i++)
            {
                validateAnimation(animation, "Attack_" + i);
            }
        }
        if (eatAnimVariantsCount == 1)
        {
            validateAnimation(animation, "Eat");
        }
        else
        {
            for (int j = 0; j < eatAnimVariantsCount; j++)
            {
                validateAnimation(animation, "Eat_" + j);
            }
        }
        for (int k = 0; k < glanceAnimVariantsCount; k++)
        {
            validateAnimation(animation, "Glance_" + k);
        }
        if (startleAnimVariantsCount == 1)
        {
            validateAnimation(animation, "Startle");
            return;
        }
        for (int l = 0; l < startleAnimVariantsCount; l++)
        {
            validateAnimation(animation, "Startle_" + l);
        }
    }

    public AnimalAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        if (id < 50 && !bundle.hasResource && !data.has("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 50");
        }
        _animalName = localization.format("Name");
        _client = bundle.load<GameObject>("Animal_Client");
        _server = bundle.load<GameObject>("Animal_Server");
        _dedicated = bundle.load<GameObject>("Animal_Dedicated");
        _ragdoll = bundle.load<GameObject>("Ragdoll");
        if (client == null)
        {
            Assets.reportError(this, "missing 'Animal_Client' GameObject. Highly recommended to fix.");
        }
        else if ((bool)Assets.shouldValidateAssets)
        {
            validateAnimations(client);
        }
        if (server == null)
        {
            Assets.reportError(this?.ToString() + "missing 'Animal_Server' GameObject. Highly recommended to fix.");
        }
        else if ((bool)Assets.shouldValidateAssets)
        {
            validateAnimations(server);
        }
        if (dedicated == null)
        {
            Assets.reportError(this, "missing 'Animal_Dedicated' GameObject. Highly recommended to fix.");
        }
        if (ragdoll == null)
        {
            Assets.reportError(this, "missing 'Ragdoll' GameObject. Highly recommended to fix.");
        }
        _speedRun = data.readSingle("Speed_Run");
        _speedWalk = data.readSingle("Speed_Walk");
        _behaviour = (EAnimalBehaviour)Enum.Parse(typeof(EAnimalBehaviour), data.readString("Behaviour"), ignoreCase: true);
        _health = data.readUInt16("Health", 0);
        _regen = data.readSingle("Regen");
        if (!data.has("Regen"))
        {
            _regen = 10f;
        }
        _damage = data.readByte("Damage", 0);
        _meat = data.readUInt16("Meat", 0);
        _pelt = data.readUInt16("Pelt", 0);
        _rewardID = data.readUInt16("Reward_ID", 0);
        if (data.has("Reward_Min"))
        {
            _rewardMin = data.readByte("Reward_Min", 0);
        }
        else
        {
            _rewardMin = 3;
        }
        if (data.has("Reward_Max"))
        {
            _rewardMax = data.readByte("Reward_Max", 0);
        }
        else
        {
            _rewardMax = 4;
        }
        _roars = new AudioClip[data.readByte("Roars", 0)];
        for (byte b = 0; b < roars.Length; b = (byte)(b + 1))
        {
            roars[b] = bundle.load<AudioClip>("Roar_" + b);
        }
        _panics = new AudioClip[data.readByte("Panics", 0)];
        for (byte b2 = 0; b2 < panics.Length; b2 = (byte)(b2 + 1))
        {
            panics[b2] = bundle.load<AudioClip>("Panic_" + b2);
        }
        attackAnimVariantsCount = data.readInt32("Attack_Anim_Variants", 1);
        eatAnimVariantsCount = data.readInt32("Eat_Anim_Variants", 1);
        glanceAnimVariantsCount = data.readInt32("Glance_Anim_Variants", 2);
        startleAnimVariantsCount = data.readInt32("Startle_Anim_Variants", 1);
        horizontalAttackRangeSquared = MathfEx.Square(data.readSingle("Horizontal_Attack_Range", 2.25f));
        horizontalVehicleAttackRangeSquared = MathfEx.Square(data.readSingle("Horizontal_Vehicle_Attack_Range", 4.4f));
        verticalAttackRange = data.readSingle("Vertical_Attack_Range", 2f);
        attackInterval = data.readSingle("Attack_Interval", 1f);
        shouldPlayAnimsOnDedicatedServer = data.readBoolean("Should_Play_Anims_On_Dedicated_Server");
        _rewardXP = data.readUInt32("Reward_XP");
    }
}
