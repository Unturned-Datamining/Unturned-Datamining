using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableSentry : InteractableStorage
{
    private static List<Player> playersInRadius = new List<Player>();

    private static List<Zombie> zombiesInRadius = new List<Zombie>();

    private static List<Animal> animalsInRadius = new List<Animal>();

    public InteractablePower power;

    private bool hasWeapon;

    private bool interact;

    private Attachments attachments;

    private AudioSource gunshotAudioSource;

    private ParticleSystem shellEmitter;

    private ParticleSystem muzzleEmitter;

    private Light muzzleLight;

    private ParticleSystem tracerEmitter;

    private Transform yawTransform;

    private Transform pitchTransform;

    private Transform aimTransform;

    private GameObject onGameObject;

    private GameObject onModelGameObject;

    private Material onMaterial;

    private GameObject offGameObject;

    private GameObject offModelGameObject;

    private Material offMaterial;

    private GameObject spotGameObject;

    private Player targetPlayer;

    private Zombie targetZombie;

    private Animal targetAnimal;

    private float targetYaw;

    private float yaw;

    private float targetPitch;

    private float pitch;

    private bool isAlert;

    private double lastAlert;

    private bool isFiring;

    private double lastFire;

    private float fireTime;

    private bool isAiming;

    private double lastAim;

    private double lastScan;

    private double lastDrift;

    private double lastShot;

    internal static readonly ClientInstanceMethod SendShoot = ClientInstanceMethod.Get(typeof(InteractableSentry), "ReceiveShoot");

    internal static readonly ClientInstanceMethod<byte, byte> SendAlert = ClientInstanceMethod<byte, byte>.Get(typeof(InteractableSentry), "ReceiveAlert");

    private bool hasInitializedSentryComponents;

    public ItemSentryAsset sentryAsset { get; private set; }

    public ESentryMode sentryMode => sentryAsset.sentryMode;

    public bool isPowered
    {
        get
        {
            if (power == null)
            {
                return false;
            }
            if (sentryAsset.requiresPower)
            {
                return power.isWired;
            }
            return true;
        }
    }

    private void trace(Vector3 pos, Vector3 dir)
    {
        if (!(tracerEmitter == null) && (!(attachments.barrelModel != null) || !attachments.barrelAsset.isBraked || displayItem.state[16] <= 0))
        {
            tracerEmitter.transform.position = pos;
            tracerEmitter.transform.rotation = Quaternion.LookRotation(dir);
            tracerEmitter.Emit(1);
        }
    }

    public void shoot()
    {
        lastAlert = Time.timeAsDouble;
        if (!Dedicator.IsDedicatedServer)
        {
            if (gunshotAudioSource != null)
            {
                AudioClip clip = ((ItemGunAsset)displayAsset).shoot;
                float num = 1f;
                float num2 = ((ItemGunAsset)displayAsset).gunshotRolloffDistance;
                if (attachments.barrelAsset != null && displayItem.state[16] > 0)
                {
                    if (attachments.barrelAsset.shoot != null)
                    {
                        clip = attachments.barrelAsset.shoot;
                    }
                    num *= attachments.barrelAsset.volume;
                    num2 *= attachments.barrelAsset.gunshotRolloffDistanceMultiplier;
                }
                gunshotAudioSource.clip = clip;
                gunshotAudioSource.volume = num;
                gunshotAudioSource.maxDistance = num2;
                gunshotAudioSource.pitch = Random.Range(0.975f, 1.025f);
                gunshotAudioSource.PlayOneShot(gunshotAudioSource.clip);
            }
            if (((ItemGunAsset)displayAsset).action == EAction.Trigger && shellEmitter != null)
            {
                shellEmitter.Emit(1);
            }
            if (attachments.barrelModel == null || !attachments.barrelAsset.isBraked || displayItem.state[16] == 0)
            {
                if (muzzleEmitter != null)
                {
                    muzzleEmitter.Emit(1);
                }
                if (muzzleLight != null)
                {
                    muzzleLight.enabled = true;
                }
            }
            if (aimTransform != null)
            {
                if (((ItemGunAsset)displayAsset).range < 32f)
                {
                    trace(aimTransform.position + aimTransform.forward * 32f, aimTransform.forward);
                }
                else
                {
                    trace(aimTransform.position + aimTransform.forward * Random.Range(32f, Mathf.Min(64f, ((ItemGunAsset)displayAsset).range)), aimTransform.forward);
                }
            }
        }
        lastShot = Time.timeAsDouble;
        if (attachments.barrelAsset != null && attachments.barrelAsset.durability > 0)
        {
            if (attachments.barrelAsset.durability > displayItem.state[16])
            {
                displayItem.state[16] = 0;
            }
            else
            {
                displayItem.state[16] -= attachments.barrelAsset.durability;
            }
        }
    }

    public void alert(float newYaw, float newPitch)
    {
        targetYaw = newYaw;
        targetPitch = newPitch;
        lastAlert = Time.timeAsDouble;
    }

    public override void updateState(Asset asset, byte[] state)
    {
        sentryAsset = asset as ItemSentryAsset;
        if (!hasInitializedSentryComponents)
        {
            hasInitializedSentryComponents = true;
            yawTransform = base.transform.Find("Yaw");
            if (yawTransform != null)
            {
                pitchTransform = yawTransform.Find("Pitch");
                if (pitchTransform != null)
                {
                    aimTransform = pitchTransform.Find("Aim");
                    Transform transform = aimTransform.Find("Spot");
                    if (transform != null)
                    {
                        spotGameObject = transform.gameObject;
                    }
                }
            }
            Transform transform2 = base.transform.FindChildRecursive("On");
            if (transform2 != null)
            {
                onGameObject = transform2.gameObject;
            }
            Transform transform3 = base.transform.FindChildRecursive("On_Model");
            if (transform3 != null)
            {
                onModelGameObject = transform3.gameObject;
                onMaterial = onModelGameObject.GetComponent<Renderer>()?.material;
            }
            Transform transform4 = base.transform.FindChildRecursive("Off");
            if (transform4 != null)
            {
                offGameObject = transform4.gameObject;
            }
            Transform transform5 = base.transform.FindChildRecursive("Off_Model");
            if (transform5 != null)
            {
                offModelGameObject = transform5.gameObject;
                offMaterial = offModelGameObject.GetComponent<Renderer>()?.material;
            }
        }
        isAlert = false;
        lastAlert = 0.0;
        targetYaw = base.transform.localRotation.eulerAngles.y;
        yaw = targetYaw;
        targetPitch = 0f;
        pitch = targetPitch;
        targetPlayer = null;
        targetAnimal = null;
        targetZombie = null;
        base.updateState(asset, state);
    }

    public override void refreshDisplay()
    {
        base.refreshDisplay();
        hasWeapon = false;
        attachments = null;
        gunshotAudioSource = null;
        destroyEffects();
        if (spotGameObject != null)
        {
            spotGameObject.SetActive(value: false);
        }
        if (displayAsset == null || displayAsset.type != EItemType.GUN || ((ItemGunAsset)displayAsset).action == EAction.String || ((ItemGunAsset)displayAsset).action == EAction.Rocket)
        {
            return;
        }
        hasWeapon = true;
        attachments = displayModel.gameObject.GetComponent<Attachments>();
        interact = displayItem.state[12] == 1;
        if (!Dedicator.IsDedicatedServer)
        {
            gunshotAudioSource = displayModel.gameObject.AddComponent<AudioSource>();
            gunshotAudioSource.clip = null;
            gunshotAudioSource.spatialBlend = 1f;
            gunshotAudioSource.rolloffMode = AudioRolloffMode.Linear;
            gunshotAudioSource.volume = 1f;
            gunshotAudioSource.minDistance = 8f;
            gunshotAudioSource.maxDistance = 256f;
            gunshotAudioSource.playOnAwake = false;
        }
        if (attachments.ejectHook != null && ((ItemGunAsset)displayAsset).action != EAction.String && ((ItemGunAsset)displayAsset).action != EAction.Rocket)
        {
            EffectAsset effectAsset = ((ItemGunAsset)displayAsset).FindShellEffectAsset();
            if (effectAsset != null)
            {
                Transform transform = EffectManager.InstantiateFromPool(effectAsset.effect).transform;
                transform.name = "Emitter";
                transform.parent = attachments.ejectHook;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                shellEmitter = transform.GetComponent<ParticleSystem>();
            }
        }
        if (attachments.barrelHook != null)
        {
            EffectAsset effectAsset2 = ((ItemGunAsset)displayAsset).FindMuzzleEffectAsset();
            if (effectAsset2 != null)
            {
                Transform transform2 = EffectManager.InstantiateFromPool(effectAsset2.effect).transform;
                transform2.name = "Emitter";
                transform2.parent = attachments.barrelHook;
                transform2.localPosition = Vector3.zero;
                transform2.localRotation = Quaternion.identity;
                muzzleEmitter = transform2.GetComponent<ParticleSystem>();
                muzzleLight = transform2.GetComponent<Light>();
                if (muzzleLight != null)
                {
                    muzzleLight.enabled = false;
                    muzzleLight.cullingMask = -2049;
                }
            }
        }
        if (muzzleEmitter != null)
        {
            if (attachments.barrelModel != null)
            {
                muzzleEmitter.transform.localPosition = Vector3.up * 0.25f;
            }
            else
            {
                muzzleEmitter.transform.localPosition = Vector3.zero;
            }
        }
        if (attachments.magazineAsset != null)
        {
            EffectAsset effectAsset3 = attachments.magazineAsset.FindTracerEffectAsset();
            if (effectAsset3 != null)
            {
                Transform transform3 = EffectManager.InstantiateFromPool(effectAsset3.effect).transform;
                transform3.name = "Tracer";
                transform3.localPosition = Vector3.zero;
                transform3.localRotation = Quaternion.identity;
                tracerEmitter = transform3.GetComponent<ParticleSystem>();
            }
        }
        if (!Dedicator.IsDedicatedServer)
        {
            if (attachments.tacticalAsset != null && (attachments.tacticalAsset.isLight || attachments.tacticalAsset.isLaser) && attachments.lightHook != null)
            {
                attachments.lightHook.gameObject.SetActive(interact);
            }
            if (spotGameObject != null)
            {
                spotGameObject.SetActive(attachments.tacticalAsset != null && attachments.tacticalAsset.isLight && interact);
            }
        }
        int num = ((ItemGunAsset)displayAsset).firerate;
        if (attachments.tacticalAsset != null)
        {
            num -= attachments.tacticalAsset.firerate;
        }
        num = Mathf.Max(num, 1);
        fireTime = num;
        fireTime /= 50f;
        fireTime *= 3.33f;
    }

    private void Update()
    {
        if (Provider.isServer && isPowered)
        {
            Vector3 vector = base.transform.position + new Vector3(0f, 0.65f, 0f);
            if (Time.timeAsDouble - lastScan > 0.10000000149011612)
            {
                lastScan = Time.timeAsDouble;
                float num = sentryAsset.detectionRadius;
                float num2 = sentryAsset.targetLossRadius;
                if (hasWeapon)
                {
                    float range = ((ItemWeaponAsset)displayAsset).range;
                    num = Mathf.Min(num, range);
                    num2 = Mathf.Min(num2, range);
                }
                float num3 = num * num;
                float num4 = num2 * num2;
                float num5 = num3;
                bool flag = false;
                Player player = null;
                Zombie zombie = null;
                Animal animal = null;
                if (Provider.isPvP)
                {
                    float sqrRadius = ((targetPlayer != null) ? num4 : num5);
                    playersInRadius.Clear();
                    PlayerTool.getPlayersInRadius(vector, sqrRadius, playersInRadius);
                    for (int i = 0; i < playersInRadius.Count; i++)
                    {
                        Player player2 = playersInRadius[i];
                        if (player2.channel.owner.playerID.steamID == base.owner || player2.quests.isMemberOfGroup(base.group) || player2.life.isDead || player2.animator.gesture == EPlayerGesture.ARREST_START || (player2.movement.isSafe && player2.movement.isSafeInfo.noWeapons) || !player2.movement.canAddSimulationResultsToUpdates || (player != null && player2.animator.gesture == EPlayerGesture.SURRENDER_START) || (sentryMode == ESentryMode.FRIENDLY && !(Time.realtimeSinceStartup - player2.equipment.lastPunching < 2f) && (!player2.equipment.isSelected || player2.equipment.asset == null || !player2.equipment.asset.shouldFriendlySentryTargetUser)))
                        {
                            continue;
                        }
                        float sqrMagnitude = (player2.look.aim.position - vector).sqrMagnitude;
                        if (player2 != targetPlayer && sqrMagnitude > num5)
                        {
                            continue;
                        }
                        Vector3 vector2 = player2.look.aim.position - vector;
                        float magnitude = vector2.magnitude;
                        Vector3 vector3 = vector2 / magnitude;
                        if (player2 != targetPlayer && Vector3.Dot(vector3, aimTransform.forward) < 0.5f)
                        {
                            continue;
                        }
                        if (magnitude > 0.025f)
                        {
                            Physics.Raycast(new Ray(vector, vector3), out var hitInfo, magnitude - 0.025f, RayMasks.BLOCK_SENTRY);
                            if (hitInfo.transform != null && hitInfo.transform != base.transform)
                            {
                                continue;
                            }
                            Physics.Raycast(new Ray(vector + vector3 * (magnitude - 0.025f), -vector3), out hitInfo, magnitude - 0.025f, RayMasks.DAMAGE_SERVER);
                            if (hitInfo.transform != null && hitInfo.transform != base.transform)
                            {
                                continue;
                            }
                        }
                        num5 = sqrMagnitude;
                        player = player2;
                        flag = true;
                    }
                }
                float sqrRadius2 = ((!flag && targetZombie != null) ? num4 : num5);
                zombiesInRadius.Clear();
                ZombieManager.getZombiesInRadius(vector, sqrRadius2, zombiesInRadius);
                for (int j = 0; j < zombiesInRadius.Count; j++)
                {
                    Zombie zombie2 = zombiesInRadius[j];
                    if (zombie2.isDead || !zombie2.isHunting)
                    {
                        continue;
                    }
                    Vector3 position = zombie2.transform.position;
                    switch (zombie2.speciality)
                    {
                    case EZombieSpeciality.CRAWLER:
                        position += new Vector3(0f, 0.25f, 0f);
                        break;
                    case EZombieSpeciality.MEGA:
                        position += new Vector3(0f, 2.625f, 0f);
                        break;
                    case EZombieSpeciality.NORMAL:
                        position += new Vector3(0f, 1.75f, 0f);
                        break;
                    case EZombieSpeciality.SPRINTER:
                        position += new Vector3(0f, 1f, 0f);
                        break;
                    }
                    float sqrMagnitude2 = (position - vector).sqrMagnitude;
                    if (zombie2 != targetZombie && sqrMagnitude2 > num5)
                    {
                        continue;
                    }
                    Vector3 vector4 = position - vector;
                    float magnitude2 = vector4.magnitude;
                    Vector3 vector5 = vector4 / magnitude2;
                    if (zombie2 != targetZombie && Vector3.Dot(vector5, aimTransform.forward) < 0.5f)
                    {
                        continue;
                    }
                    if (magnitude2 > 0.025f)
                    {
                        Physics.Raycast(new Ray(vector, vector5), out var hitInfo2, magnitude2 - 0.025f, RayMasks.BLOCK_SENTRY);
                        if (hitInfo2.transform != null && hitInfo2.transform != base.transform)
                        {
                            continue;
                        }
                        Physics.Raycast(new Ray(vector + vector5 * (magnitude2 - 0.025f), -vector5), out hitInfo2, magnitude2 - 0.025f, RayMasks.DAMAGE_SERVER);
                        if (hitInfo2.transform != null && hitInfo2.transform != base.transform)
                        {
                            continue;
                        }
                    }
                    num5 = sqrMagnitude2;
                    player = null;
                    zombie = zombie2;
                    flag = true;
                }
                float sqrRadius3 = ((!flag && targetAnimal != null) ? num4 : num5);
                animalsInRadius.Clear();
                AnimalManager.getAnimalsInRadius(vector, sqrRadius3, animalsInRadius);
                for (int k = 0; k < animalsInRadius.Count; k++)
                {
                    Animal animal2 = animalsInRadius[k];
                    if (animal2.isDead)
                    {
                        continue;
                    }
                    Vector3 position2 = animal2.transform.position;
                    float sqrMagnitude3 = (position2 - vector).sqrMagnitude;
                    if (animal2 != targetAnimal && sqrMagnitude3 > num5)
                    {
                        continue;
                    }
                    Vector3 vector6 = position2 - vector;
                    float magnitude3 = vector6.magnitude;
                    Vector3 vector7 = vector6 / magnitude3;
                    if (animal2 != targetAnimal && Vector3.Dot(vector7, aimTransform.forward) < 0.5f)
                    {
                        continue;
                    }
                    if (magnitude3 > 0.025f)
                    {
                        Physics.Raycast(new Ray(vector, vector7), out var hitInfo3, magnitude3 - 0.025f, RayMasks.BLOCK_SENTRY);
                        if (hitInfo3.transform != null && hitInfo3.transform != base.transform)
                        {
                            continue;
                        }
                        Physics.Raycast(new Ray(vector + vector7 * (magnitude3 - 0.025f), -vector7), out hitInfo3, magnitude3 - 0.025f, RayMasks.DAMAGE_SERVER);
                        if (hitInfo3.transform != null && hitInfo3.transform != base.transform)
                        {
                            continue;
                        }
                    }
                    num5 = sqrMagnitude3;
                    player = null;
                    zombie = null;
                    animal = animal2;
                }
                if (player != targetPlayer || zombie != targetZombie || animal != targetAnimal)
                {
                    targetPlayer = player;
                    targetZombie = zombie;
                    targetAnimal = animal;
                    lastFire = Time.timeAsDouble + 0.1;
                }
            }
            if (targetPlayer != null)
            {
                switch (sentryMode)
                {
                case ESentryMode.NEUTRAL:
                case ESentryMode.FRIENDLY:
                    isFiring = targetPlayer.animator.gesture != EPlayerGesture.SURRENDER_START;
                    break;
                case ESentryMode.HOSTILE:
                    isFiring = true;
                    break;
                }
                isAiming = true;
            }
            else if (targetZombie != null)
            {
                isFiring = true;
                isAiming = true;
            }
            else if (targetAnimal != null)
            {
                switch (sentryMode)
                {
                case ESentryMode.NEUTRAL:
                case ESentryMode.FRIENDLY:
                    isFiring = targetAnimal.isHunting;
                    break;
                case ESentryMode.HOSTILE:
                    isFiring = true;
                    break;
                }
                isAiming = true;
            }
            else
            {
                isFiring = false;
                isAiming = false;
            }
            if (isAiming && Time.timeAsDouble - lastAim > (double)Provider.UPDATE_TIME)
            {
                lastAim = Time.timeAsDouble;
                Transform transform = null;
                Vector3 vector8 = Vector3.zero;
                if (targetPlayer != null)
                {
                    transform = targetPlayer.transform;
                    vector8 = targetPlayer.look.aim.position;
                }
                else if (targetZombie != null)
                {
                    transform = targetZombie.transform;
                    vector8 = targetZombie.transform.position;
                    switch (targetZombie.speciality)
                    {
                    case EZombieSpeciality.CRAWLER:
                        vector8 += new Vector3(0f, 0.25f, 0f);
                        break;
                    case EZombieSpeciality.MEGA:
                        vector8 += new Vector3(0f, 2.625f, 0f);
                        break;
                    case EZombieSpeciality.NORMAL:
                        vector8 += new Vector3(0f, 1.75f, 0f);
                        break;
                    case EZombieSpeciality.SPRINTER:
                        vector8 += new Vector3(0f, 1f, 0f);
                        break;
                    }
                }
                else if (targetAnimal != null)
                {
                    transform = targetAnimal.transform;
                    vector8 = targetAnimal.transform.position + Vector3.up;
                }
                if (transform != null)
                {
                    float num6 = Mathf.Atan2(vector8.x - vector.x, vector8.z - vector.z) * 57.29578f;
                    float num7 = Mathf.Sin((vector8.y - vector.y) / (vector8 - vector).magnitude) * 57.29578f;
                    BarricadeManager.sendAlertSentry(base.transform, num6, num7);
                }
            }
            if (isFiring && hasWeapon && displayItem.state[10] > 0 && !isOpen && Time.timeAsDouble - lastFire > (double)fireTime)
            {
                lastFire += fireTime;
                if (Time.timeAsDouble - lastFire > (double)fireTime)
                {
                    lastFire = Time.timeAsDouble;
                }
                float num8 = (float)(int)displayItem.quality / 100f;
                if (attachments.magazineAsset == null)
                {
                    return;
                }
                if (!sentryAsset.infiniteAmmo && !((ItemGunAsset)displayAsset).infiniteAmmo)
                {
                    displayItem.state[10]--;
                }
                if (attachments.barrelAsset == null || !attachments.barrelAsset.isSilenced || displayItem.state[16] == 0)
                {
                    AlertTool.alert(base.transform.position, 48f);
                }
                if (!sentryAsset.infiniteQuality && Provider.modeConfigData.Items.Has_Durability && displayItem.quality > 0 && Random.value < ((ItemWeaponAsset)displayAsset).durability)
                {
                    if (displayItem.quality > ((ItemWeaponAsset)displayAsset).wear)
                    {
                        displayItem.quality -= ((ItemWeaponAsset)displayAsset).wear;
                    }
                    else
                    {
                        displayItem.quality = 0;
                    }
                }
                float baseSpreadAngleRadians = ((ItemGunAsset)displayAsset).baseSpreadAngleRadians;
                baseSpreadAngleRadians *= ((ItemGunAsset)displayAsset).spreadAim;
                baseSpreadAngleRadians *= ((num8 < 0.5f) ? (1f + (1f - num8 * 2f)) : 1f);
                if (attachments.tacticalAsset != null && interact)
                {
                    baseSpreadAngleRadians *= attachments.tacticalAsset.spread;
                }
                if (attachments.gripAsset != null)
                {
                    baseSpreadAngleRadians *= attachments.gripAsset.spread;
                }
                if (attachments.barrelAsset != null)
                {
                    baseSpreadAngleRadians *= attachments.barrelAsset.spread;
                }
                if (attachments.magazineAsset != null)
                {
                    baseSpreadAngleRadians *= attachments.magazineAsset.spread;
                }
                if (((ItemGunAsset)displayAsset).projectile == null)
                {
                    BarricadeManager.sendShootSentry(base.transform);
                    byte pellets = attachments.magazineAsset.pellets;
                    for (byte b = 0; b < pellets; b = (byte)(b + 1))
                    {
                        EPlayerKill kill = EPlayerKill.NONE;
                        uint xp = 0u;
                        float num9 = 1f;
                        num9 *= ((num8 < 0.5f) ? (0.5f + num8) : 1f);
                        Transform transform2 = null;
                        float num10 = 0f;
                        if (targetPlayer != null)
                        {
                            transform2 = targetPlayer.transform;
                        }
                        else if (targetZombie != null)
                        {
                            transform2 = targetZombie.transform;
                        }
                        else if (targetAnimal != null)
                        {
                            transform2 = targetAnimal.transform;
                        }
                        if (transform2 != null)
                        {
                            num10 = (transform2.position - base.transform.position).magnitude;
                        }
                        float num11 = num10 / ((ItemWeaponAsset)displayAsset).range;
                        num11 = 1f - num11;
                        num11 *= 1f - ((ItemGunAsset)displayAsset).spreadHip;
                        num11 *= 0.75f;
                        if (transform2 == null || Random.value > num11)
                        {
                            Vector3 randomForwardVectorInCone = RandomEx.GetRandomForwardVectorInCone(baseSpreadAngleRadians);
                            RaycastInfo raycastInfo = DamageTool.raycast(new Ray(aimTransform.position, randomForwardVectorInCone), ((ItemWeaponAsset)displayAsset).range, RayMasks.DAMAGE_SERVER);
                            if (!(raycastInfo.transform == null))
                            {
                                DamageTool.ServerSpawnBulletImpact(raycastInfo.point, raycastInfo.normal, raycastInfo.materialName, raycastInfo.collider?.transform, Provider.EnumerateClients_WithinSphere(raycastInfo.point, EffectManager.SMALL));
                                if (raycastInfo.vehicle != null)
                                {
                                    DamageTool.damage(raycastInfo.vehicle, damageTires: false, Vector3.zero, isRepairing: false, ((ItemGunAsset)displayAsset).vehicleDamage, num9, canRepair: true, out kill, default(CSteamID), EDamageOrigin.Sentry);
                                }
                                else if (raycastInfo.transform != null)
                                {
                                    if (raycastInfo.transform.CompareTag("Barricade"))
                                    {
                                        BarricadeDrop barricadeDrop = BarricadeDrop.FindByRootFast(raycastInfo.transform);
                                        if (barricadeDrop != null)
                                        {
                                            ItemBarricadeAsset asset = barricadeDrop.asset;
                                            if (asset != null && asset.canBeDamaged && (asset.isVulnerable || ((ItemWeaponAsset)displayAsset).isInvulnerable))
                                            {
                                                DamageTool.damage(raycastInfo.transform, isRepairing: false, ((ItemGunAsset)displayAsset).barricadeDamage, num9, out kill, default(CSteamID), EDamageOrigin.Sentry);
                                            }
                                        }
                                    }
                                    else if (raycastInfo.transform.CompareTag("Structure"))
                                    {
                                        StructureDrop structureDrop = StructureDrop.FindByRootFast(raycastInfo.transform);
                                        if (structureDrop != null)
                                        {
                                            ItemStructureAsset asset2 = structureDrop.asset;
                                            if (asset2 != null && asset2.canBeDamaged && (asset2.isVulnerable || ((ItemWeaponAsset)displayAsset).isInvulnerable))
                                            {
                                                DamageTool.damage(raycastInfo.transform, isRepairing: false, raycastInfo.direction * Mathf.Ceil((float)(int)attachments.magazineAsset.pellets / 2f), ((ItemGunAsset)displayAsset).structureDamage, num9, out kill, default(CSteamID), EDamageOrigin.Sentry);
                                            }
                                        }
                                    }
                                    else if (raycastInfo.transform.CompareTag("Resource"))
                                    {
                                        if (ResourceManager.tryGetRegion(raycastInfo.transform, out var x, out var y, out var index))
                                        {
                                            ResourceSpawnpoint resourceSpawnpoint = ResourceManager.getResourceSpawnpoint(x, y, index);
                                            if (resourceSpawnpoint != null && !resourceSpawnpoint.isDead && ((ItemWeaponAsset)displayAsset).hasBladeID(resourceSpawnpoint.asset.bladeID))
                                            {
                                                DamageTool.damage(raycastInfo.transform, raycastInfo.direction * Mathf.Ceil((float)(int)attachments.magazineAsset.pellets / 2f), ((ItemGunAsset)displayAsset).resourceDamage, num9, 1f, out kill, out xp, default(CSteamID), EDamageOrigin.Sentry);
                                            }
                                        }
                                    }
                                    else if (raycastInfo.section < byte.MaxValue)
                                    {
                                        InteractableObjectRubble componentInParent = raycastInfo.transform.GetComponentInParent<InteractableObjectRubble>();
                                        if (componentInParent != null && componentInParent.IsSectionIndexValid(raycastInfo.section) && !componentInParent.isSectionDead(raycastInfo.section) && ((ItemWeaponAsset)displayAsset).hasBladeID(componentInParent.asset.rubbleBladeID) && (componentInParent.asset.rubbleIsVulnerable || ((ItemWeaponAsset)displayAsset).isInvulnerable))
                                        {
                                            DamageTool.damage(componentInParent.transform, raycastInfo.direction, raycastInfo.section, ((ItemGunAsset)displayAsset).objectDamage, num9, out kill, out xp, default(CSteamID), EDamageOrigin.Sentry);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Vector3 position3 = Vector3.zero;
                            if (targetPlayer != null)
                            {
                                position3 = targetPlayer.look.aim.position;
                            }
                            else if (targetZombie != null)
                            {
                                position3 = targetZombie.transform.position;
                                switch (targetZombie.speciality)
                                {
                                case EZombieSpeciality.CRAWLER:
                                    position3 += new Vector3(0f, 0.25f, 0f);
                                    break;
                                case EZombieSpeciality.MEGA:
                                    position3 += new Vector3(0f, 2.625f, 0f);
                                    break;
                                case EZombieSpeciality.NORMAL:
                                    position3 += new Vector3(0f, 1.75f, 0f);
                                    break;
                                case EZombieSpeciality.SPRINTER:
                                    position3 += new Vector3(0f, 1f, 0f);
                                    break;
                                }
                            }
                            else if (targetAnimal != null)
                            {
                                position3 = targetAnimal.transform.position + Vector3.up;
                            }
                            DamageTool.ServerSpawnBulletImpact(position3, -aimTransform.forward, "Flesh_Dynamic", null, Provider.EnumerateClients_WithinSphere(position3, EffectManager.SMALL));
                            Vector3 direction = aimTransform.forward * Mathf.Ceil((float)(int)attachments.magazineAsset.pellets / 2f);
                            if (targetPlayer != null)
                            {
                                DamageTool.damage(targetPlayer, EDeathCause.SENTRY, ELimb.SPINE, base.owner, direction, ((ItemGunAsset)displayAsset).playerDamageMultiplier, num9, armor: true, out kill, trackKill: true);
                            }
                            else if (targetZombie != null)
                            {
                                IDamageMultiplier zombieOrPlayerDamageMultiplier = ((ItemGunAsset)displayAsset).zombieOrPlayerDamageMultiplier;
                                DamageZombieParameters parameters = DamageZombieParameters.make(targetZombie, direction, zombieOrPlayerDamageMultiplier, ELimb.SPINE);
                                parameters.times = num9;
                                parameters.allowBackstab = false;
                                parameters.respectArmor = true;
                                parameters.instigator = this;
                                DamageTool.damageZombie(parameters, out kill, out xp);
                            }
                            else if (targetAnimal != null)
                            {
                                IDamageMultiplier animalOrPlayerDamageMultiplier = ((ItemGunAsset)displayAsset).animalOrPlayerDamageMultiplier;
                                DamageAnimalParameters parameters2 = DamageAnimalParameters.make(targetAnimal, direction, animalOrPlayerDamageMultiplier, ELimb.SPINE);
                                parameters2.times = num9;
                                parameters2.instigator = this;
                                DamageTool.damageAnimal(parameters2, out kill, out xp);
                            }
                        }
                    }
                }
                rebuildState();
            }
        }
        bool flag2 = Time.timeAsDouble - lastAlert < 1.0;
        if (flag2 != isAlert)
        {
            isAlert = flag2;
            if (!Dedicator.IsDedicatedServer)
            {
                if (isAlert)
                {
                    EffectManager.effect(sentryAsset.targetAcquiredEffect, base.transform.position);
                }
                else
                {
                    EffectManager.effect(sentryAsset.targetLostEffect, base.transform.position);
                }
            }
            if (!isAlert)
            {
                targetYaw = base.transform.localRotation.eulerAngles.y;
            }
        }
        if (!(power != null))
        {
            return;
        }
        if (isPowered)
        {
            if (isAlert)
            {
                lastDrift = Time.timeAsDouble;
                yaw = Mathf.LerpAngle(yaw, targetYaw, 4f * Time.deltaTime);
            }
            else
            {
                yaw = Mathf.LerpAngle(yaw, targetYaw + Mathf.Sin((float)(Time.timeAsDouble - lastDrift)) * 60f, 4f * Time.deltaTime);
            }
            pitch = Mathf.LerpAngle(pitch, targetPitch, 4f * Time.deltaTime);
            if (yawTransform != null)
            {
                yawTransform.rotation = Quaternion.Euler(-90f, 0f, yaw);
            }
            if (pitchTransform != null)
            {
                pitchTransform.localRotation = Quaternion.Euler(0f, -90f, pitch);
            }
        }
        if (onGameObject != null)
        {
            onGameObject.SetActive(isAlert && isPowered);
        }
        if (onModelGameObject != null)
        {
            onModelGameObject.SetActive(isAlert);
        }
        if (offGameObject != null)
        {
            offGameObject.SetActive(!isAlert && isPowered);
        }
        if (offModelGameObject != null)
        {
            offModelGameObject.SetActive(!isAlert);
        }
        if (!Dedicator.IsDedicatedServer)
        {
            if (onMaterial != null)
            {
                onMaterial.SetColor("_EmissionColor", (isAlert && isPowered) ? (onMaterial.color * 2f) : Color.black);
            }
            if (offMaterial != null)
            {
                offMaterial.SetColor("_EmissionColor", (!isAlert && isPowered) ? (offMaterial.color * 2f) : Color.black);
            }
            if (Time.timeAsDouble - lastShot > 0.05 && muzzleLight != null)
            {
                muzzleLight.GetComponent<Light>().enabled = false;
            }
        }
    }

    private void destroyEffects()
    {
        if (tracerEmitter != null)
        {
            EffectManager.DestroyIntoPool(tracerEmitter.gameObject);
            tracerEmitter = null;
        }
        if (muzzleEmitter != null)
        {
            EffectManager.DestroyIntoPool(muzzleEmitter.gameObject);
            muzzleEmitter = null;
        }
        muzzleLight = null;
        if (shellEmitter != null)
        {
            EffectManager.DestroyIntoPool(shellEmitter.gameObject);
            shellEmitter = null;
        }
    }

    private void OnDestroy()
    {
        destroyEffects();
        if (onMaterial != null)
        {
            Object.DestroyImmediate(onMaterial);
            onMaterial = null;
        }
        if (offMaterial != null)
        {
            Object.DestroyImmediate(offMaterial);
            offMaterial = null;
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveShoot()
    {
        shoot();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveAlert(byte yaw, byte pitch)
    {
        alert(MeasurementTool.byteToAngle(yaw), MeasurementTool.byteToAngle(pitch));
    }
}
