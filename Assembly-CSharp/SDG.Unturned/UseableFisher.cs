using System;
using SDG.Framework.Water;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableFisher : Useable
{
    private float startedCast;

    private float startedReel;

    private float castTime;

    private float reelTime;

    private bool isStrengthening;

    private bool isCasting;

    private bool isReeling;

    private bool isFishing;

    private bool isBobbing;

    private bool isLuring;

    private bool isCatch;

    private Transform bobberTransform;

    private Rigidbody bobberRigidbody;

    private Transform firstHook;

    private Transform thirdHook;

    private LineRenderer firstLine;

    private LineRenderer thirdLine;

    private Vector3 water;

    private uint strengthTime;

    private float strengthMultiplier;

    private float lastLuck;

    private float luckTime;

    private bool hasLuckReset;

    private bool hasSplashed;

    private bool hasTugged;

    private ISleekBox castStrengthBox;

    private ISleekElement castStrengthArea;

    private ISleekImage castStrengthBar;

    private static readonly ServerInstanceMethod SendCatch = ServerInstanceMethod.Get(typeof(UseableFisher), "ReceiveCatch");

    private static readonly ClientInstanceMethod<float> SendLuckTime = ClientInstanceMethod<float>.Get(typeof(UseableFisher), "ReceiveLuckTime");

    private static readonly ClientInstanceMethod SendPlayReel = ClientInstanceMethod.Get(typeof(UseableFisher), "ReceivePlayReel");

    private static readonly ClientInstanceMethod SendPlayCast = ClientInstanceMethod.Get(typeof(UseableFisher), "ReceivePlayCast");

    public override bool isUseableShowingMenu
    {
        get
        {
            if (castStrengthBox != null)
            {
                return castStrengthBox.IsVisible;
            }
            return false;
        }
    }

    private bool isCastable => Time.realtimeSinceStartup - startedCast > castTime;

    private bool isReelable => Time.realtimeSinceStartup - startedReel > reelTime;

    private bool isBobable
    {
        get
        {
            if (!isCasting)
            {
                return Time.realtimeSinceStartup - startedReel > reelTime * 0.75f;
            }
            return Time.realtimeSinceStartup - startedCast > castTime * 0.45f;
        }
    }

    private void reel()
    {
        if (!Dedicator.IsDedicatedServer)
        {
            base.player.playSound(((ItemFisherAsset)base.player.equipment.asset).reel);
        }
        base.player.animator.play("Reel", smooth: false);
    }

    private void startStrength()
    {
        PlayerLifeUI.close();
        if (castStrengthBox != null)
        {
            castStrengthBox.IsVisible = true;
        }
    }

    private void stopStrength()
    {
        PlayerLifeUI.open();
        if (castStrengthBox != null)
        {
            castStrengthBox.IsVisible = false;
        }
    }

    [Obsolete]
    public void askCatch(CSteamID steamID)
    {
        ReceiveCatch();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10, legacyName = "askCatch")]
    public void ReceiveCatch()
    {
        if (Time.realtimeSinceStartup - lastLuck > luckTime - 2.4f || (hasLuckReset && Time.realtimeSinceStartup - lastLuck < 1f))
        {
            isCatch = true;
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveLuckTime(float NewLuckTime)
    {
        luckTime = NewLuckTime;
    }

    [Obsolete]
    public void askReel(CSteamID steamID)
    {
        ReceivePlayReel();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askReel")]
    public void ReceivePlayReel()
    {
        if (base.player.equipment.IsEquipAnimationFinished)
        {
            reel();
        }
    }

    private void cast()
    {
        if (!Dedicator.IsDedicatedServer)
        {
            base.player.playSound(((ItemFisherAsset)base.player.equipment.asset).cast);
        }
        base.player.animator.play("Cast", smooth: false);
    }

    [Obsolete]
    public void askCast(CSteamID steamID)
    {
        ReceivePlayCast();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askCast")]
    public void ReceivePlayCast()
    {
        if (base.player.equipment.IsEquipAnimationFinished)
        {
            cast();
        }
    }

    public override bool startPrimary()
    {
        if (base.player.equipment.isBusy)
        {
            return false;
        }
        if (isFishing)
        {
            isFishing = false;
            base.player.equipment.isBusy = true;
            startedReel = Time.realtimeSinceStartup;
            isReeling = true;
            if (base.channel.IsLocalPlayer)
            {
                isBobbing = true;
                if (bobberTransform != null && !isLuring && Time.realtimeSinceStartup - lastLuck > luckTime - 1.4f && Time.realtimeSinceStartup - lastLuck < luckTime)
                {
                    SendCatch.Invoke(GetNetId(), ENetReliability.Reliable);
                }
            }
            reel();
            if (Provider.isServer)
            {
                if (isCatch)
                {
                    isCatch = false;
                    ItemFisherAsset itemFisherAsset = (ItemFisherAsset)base.player.equipment.asset;
                    ushort num = SpawnTableTool.ResolveLegacyId(itemFisherAsset.rewardID, EAssetType.ITEM, OnGetRewardErrorContext);
                    if (num != 0)
                    {
                        base.player.inventory.forceAddItem(new Item(num, EItemOrigin.NATURE), auto: false);
                    }
                    base.player.sendStat(EPlayerStat.FOUND_FISHES);
                    int num2 = UnityEngine.Random.Range(itemFisherAsset.rewardExperienceMin, itemFisherAsset.rewardExperienceMax + 1);
                    if (num2 > 0)
                    {
                        base.player.skills.askPay((uint)num2);
                    }
                    itemFisherAsset.rewardsList.Grant(base.player);
                }
                SendPlayReel.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GatherRemoteClientConnectionsExcludingOwner());
                AlertTool.alert(base.transform.position, 8f);
            }
        }
        else
        {
            isStrengthening = true;
            strengthTime = 0u;
            strengthMultiplier = 0f;
            if (base.channel.IsLocalPlayer)
            {
                startStrength();
            }
        }
        return true;
    }

    public override void stopPrimary()
    {
        if (!base.player.equipment.isBusy && isStrengthening)
        {
            isStrengthening = false;
            if (base.channel.IsLocalPlayer)
            {
                stopStrength();
            }
            isFishing = true;
            base.player.equipment.isBusy = true;
            startedCast = Time.realtimeSinceStartup;
            isCasting = true;
            if (base.channel.IsLocalPlayer)
            {
                isBobbing = true;
            }
            resetLuck();
            hasLuckReset = false;
            cast();
            if (Provider.isServer)
            {
                SendPlayCast.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GatherRemoteClientConnectionsExcludingOwner());
                AlertTool.alert(base.transform.position, 8f);
            }
        }
    }

    public override void equip()
    {
        base.player.animator.play("Equip", smooth: true);
        castTime = base.player.animator.GetAnimationLength("Cast");
        reelTime = base.player.animator.GetAnimationLength("Reel");
        if (base.channel.IsLocalPlayer)
        {
            firstHook = base.player.equipment.firstModel.Find("Hook");
            thirdHook = base.player.equipment.thirdModel.Find("Hook");
            firstLine = (LineRenderer)base.player.equipment.firstModel.Find("Line").GetComponent<Renderer>();
            firstLine.tag = "Viewmodel";
            firstLine.gameObject.layer = 11;
            firstLine.gameObject.SetActive(value: true);
            thirdLine = (LineRenderer)base.player.equipment.thirdModel.Find("Line").GetComponent<Renderer>();
            thirdLine.gameObject.SetActive(value: true);
            castStrengthBox = Glazier.Get().CreateBox();
            castStrengthBox.PositionOffset_X = -20f;
            castStrengthBox.PositionOffset_Y = -110f;
            castStrengthBox.PositionScale_X = 0.5f;
            castStrengthBox.PositionScale_Y = 0.5f;
            castStrengthBox.SizeOffset_X = 40f;
            castStrengthBox.SizeOffset_Y = 220f;
            PlayerUI.container.AddChild(castStrengthBox);
            castStrengthBox.IsVisible = false;
            castStrengthArea = Glazier.Get().CreateFrame();
            castStrengthArea.PositionOffset_X = 10f;
            castStrengthArea.PositionOffset_Y = 10f;
            castStrengthArea.SizeOffset_X = -20f;
            castStrengthArea.SizeOffset_Y = -20f;
            castStrengthArea.SizeScale_X = 1f;
            castStrengthArea.SizeScale_Y = 1f;
            castStrengthBox.AddChild(castStrengthArea);
            castStrengthBar = Glazier.Get().CreateImage();
            castStrengthBar.SizeScale_X = 1f;
            castStrengthBar.SizeScale_Y = 1f;
            castStrengthBar.Texture = (Texture2D)GlazierResources.PixelTexture;
            castStrengthArea.AddChild(castStrengthBar);
        }
    }

    public override void dequip()
    {
        if (base.channel.IsLocalPlayer)
        {
            if (bobberTransform != null)
            {
                UnityEngine.Object.Destroy(bobberTransform.gameObject);
            }
            if (castStrengthBox != null)
            {
                PlayerUI.container.RemoveChild(castStrengthBox);
            }
            if (isStrengthening)
            {
                PlayerLifeUI.open();
            }
        }
    }

    public override void tock(uint clock)
    {
        if (isStrengthening)
        {
            strengthTime++;
            uint num = (uint)(100 + base.player.skills.skills[2][4].level * 20);
            strengthMultiplier = 1f - Mathf.Abs(Mathf.Sin((float)((strengthTime + num / 2) % num) / (float)num * MathF.PI));
            strengthMultiplier *= strengthMultiplier;
            if (base.channel.IsLocalPlayer && castStrengthBar != null)
            {
                castStrengthBar.PositionScale_Y = 1f - strengthMultiplier;
                castStrengthBar.SizeScale_Y = strengthMultiplier;
                castStrengthBar.TintColor = ItemTool.getQualityColor(strengthMultiplier);
            }
        }
    }

    public override void tick()
    {
        if (!base.player.equipment.IsEquipAnimationFinished || !base.channel.IsLocalPlayer)
        {
            return;
        }
        if (isBobable && isBobbing)
        {
            if (isCasting)
            {
                Vector3 position = base.player.look.aim.position;
                Vector3 forward = base.player.look.aim.forward;
                if (Physics.Raycast(new Ray(position, forward), out var hitInfo, 1.5f, RayMasks.DAMAGE_SERVER))
                {
                    position += forward * (hitInfo.distance - 0.5f);
                }
                else
                {
                    position += forward;
                }
                bobberTransform = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Fishers/Bob")).transform;
                bobberTransform.name = "Bob";
                bobberTransform.position = position;
                bobberRigidbody = bobberTransform.GetComponent<Rigidbody>();
                if (bobberRigidbody != null)
                {
                    bobberRigidbody.AddForce(forward * Mathf.Lerp(500f, 1000f, strengthMultiplier));
                    bobberRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                }
                isBobbing = false;
                isLuring = true;
            }
            else if (isReeling && bobberTransform != null)
            {
                UnityEngine.Object.Destroy(bobberTransform.gameObject);
            }
        }
        if (bobberTransform != null)
        {
            if (base.player.look.perspective == EPlayerPerspective.FIRST)
            {
                Vector3 position2 = MainCamera.instance.WorldToViewportPoint(bobberTransform.position);
                Vector3 position3 = base.player.animator.viewmodelCamera.ViewportToWorldPoint(position2);
                firstLine.SetPosition(0, firstHook.position);
                firstLine.SetPosition(1, position3);
            }
            else
            {
                thirdLine.SetPosition(0, thirdHook.position);
                thirdLine.SetPosition(1, bobberTransform.position);
            }
        }
        else if (base.player.look.perspective == EPlayerPerspective.FIRST)
        {
            firstLine.SetPosition(0, Vector3.zero);
            firstLine.SetPosition(1, Vector3.zero);
        }
        else
        {
            thirdLine.SetPosition(0, Vector3.zero);
            thirdLine.SetPosition(1, Vector3.zero);
        }
    }

    public override void simulate(uint simulation, bool inputSteady)
    {
        if (isCasting && isCastable)
        {
            base.player.equipment.isBusy = false;
            isCasting = false;
        }
        else if (isReeling && isReelable)
        {
            base.player.equipment.isBusy = false;
            isReeling = false;
        }
        if (!base.channel.IsLocalPlayer && Time.realtimeSinceStartup - lastLuck > luckTime && !isReeling)
        {
            resetLuck();
            hasLuckReset = true;
        }
    }

    private void resetLuck()
    {
        lastLuck = Time.realtimeSinceStartup;
        if (Provider.isServer)
        {
            luckTime = UnityEngine.Random.Range(50.2f, 60.2f) - strengthMultiplier * 33.5f;
            SendLuckTime.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), luckTime);
        }
        hasSplashed = false;
        hasTugged = false;
    }

    private void Update()
    {
        if (!(bobberTransform != null) || !(bobberRigidbody != null))
        {
            return;
        }
        if (isLuring)
        {
            WaterUtility.getUnderwaterInfo(bobberTransform.position, out var isUnderwater, out var surfaceElevation);
            if (isUnderwater && bobberTransform.position.y < surfaceElevation - 4f)
            {
                bobberRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                bobberRigidbody.useGravity = false;
                bobberRigidbody.isKinematic = true;
                water = bobberTransform.position;
                water.y = surfaceElevation;
                isLuring = false;
            }
            return;
        }
        if (Time.realtimeSinceStartup - lastLuck > luckTime)
        {
            if (!isReeling)
            {
                resetLuck();
                hasLuckReset = true;
            }
        }
        else if (Time.realtimeSinceStartup - lastLuck > luckTime - 1.4f)
        {
            if (!hasTugged)
            {
                hasTugged = true;
                base.player.playSound(((ItemFisherAsset)base.player.equipment.asset).tug);
                base.player.animator.play("Tug", smooth: false);
            }
        }
        else if (Time.realtimeSinceStartup - lastLuck > luckTime - 2.4f && !hasSplashed)
        {
            hasSplashed = true;
            Transform obj = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("Fishers/Splash"))).transform;
            obj.name = "Splash";
            EffectManager.RegisterDebris(obj.gameObject);
            obj.position = water;
            obj.rotation = Quaternion.Euler(-90f, UnityEngine.Random.Range(0f, 360f), 0f);
            UnityEngine.Object.Destroy(obj.gameObject, 8f);
        }
        if (Time.realtimeSinceStartup - lastLuck > luckTime - 1.4f)
        {
            bobberRigidbody.MovePosition(Vector3.Lerp(bobberTransform.position, water + Vector3.down * 4f + Vector3.left * UnityEngine.Random.Range(-4f, 4f) + Vector3.forward * UnityEngine.Random.Range(-4f, 4f), 4f * Time.deltaTime));
        }
        else
        {
            bobberRigidbody.MovePosition(Vector3.Lerp(bobberTransform.position, water + Vector3.up * Mathf.Sin(Time.time) * 0.25f, 4f * Time.deltaTime));
        }
    }

    private string OnGetRewardErrorContext()
    {
        return "fishing " + base.player.equipment.asset?.FriendlyName + " reward";
    }
}
