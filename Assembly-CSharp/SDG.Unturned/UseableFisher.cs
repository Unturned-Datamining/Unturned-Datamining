using System;
using System.Diagnostics;
using SDG.Framework.Water;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableFisher : Useable
{
    private enum EFishingState
    {
        /// <summary>
        /// Standing with the rod out, not using it.
        /// </summary>
        Idle,
        /// <summary>
        /// Strength gauge is active.
        /// </summary>
        PreparingToCast,
        /// <summary>
        /// Bobber is floating in the water.
        /// The line is out and can be reeled back in.
        /// </summary>
        LineDeployed,
        /// <summary>
        /// Only applicable for fishing rods which opt-in.
        /// Player is doing the challenge before the item is received.
        /// </summary>
        CatchChallenge
    }

    private float startedCast;

    private float startedReel;

    private float castAnimationLength;

    private float reelAnimationLength;

    private bool isPlayingCastAnimation;

    private bool isPlayingReelAnimation;

    private EFishingState fishingState;

    /// <summary>
    /// If true, bobber will spawn or destroy once animation trigger is reached.
    /// </summary>
    private bool isWaitingForAnimationTrigger;

    /// <summary>
    /// If false, bobber has started floating.
    /// </summary>
    private bool isWaitingForBobberToFindWater;

    /// <summary>
    /// Server decides which item will be caught next.
    /// Client is notified shortly before they can catch the item.
    /// Used in the challenge UI on the client.
    /// </summary>
    private CachingAssetRef nextRewardItem;

    /// <summary>
    /// Server sends a random seed for challenge.
    /// </summary>
    private int nextRewardSeed;

    private WaterVolume serverWaterVolume;

    private bool serverHasClientConfirmedCatch;

    private Transform bobberTransform;

    private Rigidbody bobberRigidbody;

    private Transform firstHook;

    private Transform thirdHook;

    private LineRenderer firstLine;

    private LineRenderer thirdLine;

    private Vector3 waterSurfacePosition;

    private uint strengthTime;

    private float strengthMultiplier;

    private int ticksUntilFishRelocates;

    private int fishTargetPosition;

    private int fishPosition;

    private int fishVelocity;

    /// <summary>
    /// Position of challenge player cursor.
    /// </summary>
    private int challengeInputPosition;

    /// <summary>
    /// Velocity of fishing challenge player cursor.
    /// </summary>
    private int challengeInputVelocity;

    private int challengeCaptureProgress;

    private int challengeCaptureProgressPerTick;

    private int challengeEscapeProgressPerTick;

    private bool challengeInputWantsToPullUp;

    /// <summary>
    /// Decreased until a notification is sent to the client they can catch a fish.
    /// </summary>
    private float serverTimeUntilFishAppears;

    private bool serverHasSentFishNotification;

    /// <summary>
    /// Increased after fish notification is sent/received.
    /// </summary>
    private float timeSinceFishNotification = 999f;

    /// <summary>
    /// Whether animation to indicate the fish can be caught has played yet.
    /// </summary>
    private bool hasPlayedTugAnimation;

    private ISleekBox castStrengthBox;

    private ISleekElement castStrengthArea;

    private ISleekImage castStrengthBar;

    private ISleekBox challengeBox;

    private ISleekImage challengeWater;

    private ISleekImage challengeCursor;

    private ISleekElement challengeProgressBarContainer;

    private ISleekImage challengeSuccessBar;

    private ISleekImage challengeFailureBar;

    private SleekItemIcon challengePrizeIcon;

    private FishingCatchableProperties catchableProperties;

    private static AudioReference fishingLoopAudioRef = new AudioReference("core.masterbundle", "Sounds/Fishing/FishingChallengeLoop.wav");

    private static AudioReference fishingFailureAudioRef = new AudioReference("core.masterbundle", "Sounds/Fishing/FishingChallengeFailure.wav");

    private static AudioReference fishingSuccessAudioRef = new AudioReference("core.masterbundle", "Sounds/Fishing/FishingChallengeSuccess.asset");

    private OneShotAudioHandle fishingLoopAudioHandle;

    /// <summary>
    /// If true, this item has closed PlayerLifeUI.
    /// </summary>
    private bool hasClosedMainHud;

    private static readonly ServerInstanceMethod<NetId> SendBobberInWaterConfirmation = ServerInstanceMethod<NetId>.Get(typeof(UseableFisher), "ReceiveBobberInWaterConfirmation");

    private static readonly ServerInstanceMethod SendCatchConfirmation = ServerInstanceMethod.Get(typeof(UseableFisher), "ReceiveCatchConfirmation");

    private static readonly ClientInstanceMethod<Guid, int> SendFishNotification = ClientInstanceMethod<Guid, int>.Get(typeof(UseableFisher), "ReceiveFishNotification");

    private static readonly ClientInstanceMethod SendPlayReel = ClientInstanceMethod.Get(typeof(UseableFisher), "ReceivePlayReel");

    private static readonly ClientInstanceMethod SendPlayCast = ClientInstanceMethod.Get(typeof(UseableFisher), "ReceivePlayCast");

    private const float WARNING_DURATION = 1f;

    private const float CATCH_WINDOW = 1.4f;

    private const float SERVER_LENIENCY_WINDOW = 1f;

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

    private bool HasFinishedCastAnimation => Time.realtimeSinceStartup - startedCast > castAnimationLength;

    private bool HasFinishedReelAnimation => Time.realtimeSinceStartup - startedReel > reelAnimationLength;

    /// <summary>
    /// If true, enough time passed since starting Cast or Reel animation to apply its effects (e.g., spawning projectile).
    /// </summary>
    private bool HasReachedAnimationTrigger
    {
        get
        {
            if (!isPlayingCastAnimation)
            {
                return Time.realtimeSinceStartup - startedReel > reelAnimationLength * 0.75f;
            }
            return Time.realtimeSinceStartup - startedCast > castAnimationLength * 0.45f;
        }
    }

    private void SetPlayingFishingLoop(bool playing)
    {
        if (playing)
        {
            if (!fishingLoopAudioHandle.IsValid)
            {
                OneShotAudioParameters oneShotAudioParameters = new OneShotAudioParameters(base.transform, fishingLoopAudioRef);
                oneShotAudioParameters.looping = true;
                fishingLoopAudioHandle = oneShotAudioParameters.Play();
            }
        }
        else
        {
            fishingLoopAudioHandle.Stop();
        }
    }

    private void PlayFishingFailure()
    {
        OneShotAudioParameters oneShotAudioParameters = new OneShotAudioParameters(base.transform.position, fishingFailureAudioRef);
        oneShotAudioParameters.RandomizePitch(0.95f, 1.05f);
        oneShotAudioParameters.RandomizeVolume(0.95f, 1.05f);
        oneShotAudioParameters.Play();
    }

    private void PlayFishingSuccess()
    {
        OneShotAudioParameters oneShotAudioParameters = new OneShotAudioParameters(waterSurfacePosition, fishingSuccessAudioRef);
        oneShotAudioParameters.RandomizePitch(0.95f, 1.05f);
        oneShotAudioParameters.RandomizeVolume(0.95f, 1.05f);
        oneShotAudioParameters.Play();
    }

    private void PlayReelAnimation()
    {
        if (!Dedicator.IsDedicatedServer)
        {
            base.player.playSound(((ItemFisherAsset)base.player.equipment.asset).reel);
        }
        base.player.animator.play("Reel", smooth: false);
    }

    private void UpdateCastStrengthGaugeVisible(bool visible)
    {
        castStrengthBox.IsVisible = visible;
        bool isVisible = castStrengthBox.IsVisible;
        if (hasClosedMainHud != visible)
        {
            hasClosedMainHud = visible;
            if (isVisible)
            {
                PlayerLifeUI.close();
            }
            else
            {
                PlayerLifeUI.open();
            }
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10)]
    public void ReceiveBobberInWaterConfirmation(in ServerInvocationContext context, NetId waterVolumeNetId)
    {
        serverWaterVolume = NetIdRegistry.Get<WaterVolume>(waterVolumeNetId);
        if (serverWaterVolume == null)
        {
            serverWaterVolume = WaterVolumeManager.seaLevelVolume;
        }
        ResetTimeUntilFishAppears();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10)]
    public void ReceiveCatchConfirmation(in ServerInvocationContext context)
    {
        if (serverHasSentFishNotification && timeSinceFishNotification <= 3.4f)
        {
            serverHasClientConfirmedCatch = true;
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveFishNotification(Guid nextRewardGuid, int newSeed)
    {
        timeSinceFishNotification = 0f;
        hasPlayedTugAnimation = false;
        nextRewardItem = nextRewardGuid;
        nextRewardSeed = newSeed;
        if (!Dedicator.IsDedicatedServer)
        {
            Quaternion rotation = Quaternion.Euler(-90f, UnityEngine.Random.Range(0f, 360f), 0f);
            Transform obj = UnityEngine.Object.Instantiate(Assets.coreMasterBundle.LoadAsset<GameObject>("Fishers/Splash.prefab"), waterSurfacePosition, rotation).transform;
            obj.name = "Splash";
            EffectManager.RegisterDebris(obj.gameObject);
            UnityEngine.Object.Destroy(obj.gameObject, 8f);
        }
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
            PlayReelAnimation();
        }
    }

    private void PlayCastAnimation()
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
            PlayCastAnimation();
        }
    }

    public override bool startPrimary()
    {
        if (base.player.equipment.isBusy)
        {
            return false;
        }
        if (fishingState == EFishingState.Idle)
        {
            fishingState = EFishingState.PreparingToCast;
            strengthTime = 0u;
            strengthMultiplier = 0f;
            if (base.channel.IsLocalPlayer)
            {
                UpdateCastStrengthGaugeVisible(visible: true);
            }
        }
        else if (fishingState == EFishingState.LineDeployed)
        {
            ItemFisherAsset equippedAsset = GetEquippedAsset<ItemFisherAsset>();
            bool flag = serverHasClientConfirmedCatch;
            if (base.channel.IsLocalPlayer && timeSinceFishNotification >= 1f && timeSinceFishNotification <= 2.4f)
            {
                SendCatchConfirmation.Invoke(GetNetId(), ENetReliability.Reliable);
                flag = true;
            }
            serverHasClientConfirmedCatch = false;
            if (equippedAsset != null && equippedAsset.EnableCatchChallenge && (Provider.modeConfigData?.Gameplay?.Enable_Fishing_Catch_Challenge).GetValueOrDefault())
            {
                if (flag)
                {
                    fishingState = EFishingState.CatchChallenge;
                    base.player.animator.play("Catch_Loop", smooth: false);
                    ItemAsset itemAsset = nextRewardItem.Get<ItemAsset>();
                    if (itemAsset != null && itemAsset.FishingCatchable != null)
                    {
                        catchableProperties = itemAsset.FishingCatchable;
                    }
                    else
                    {
                        catchableProperties = FishingCatchableProperties.Default;
                    }
                    ticksUntilFishRelocates = 0;
                    UnityEngine.Random.State state = UnityEngine.Random.state;
                    UnityEngine.Random.InitState(nextRewardSeed);
                    fishTargetPosition = UnityEngine.Random.Range(catchableProperties.minTargetPosition, catchableProperties.maxTargetPosition + 1);
                    UnityEngine.Random.state = state;
                    fishPosition = fishTargetPosition;
                    fishVelocity = 0;
                    challengeCaptureProgress = 0;
                    float num = base.player.skills.mastery(2, 4);
                    challengeCaptureProgressPerTick = Mathf.RoundToInt(10000f * (1f + num * 0.2f) * equippedAsset.CatchChallengeCaptureSpeedMultiplier);
                    challengeEscapeProgressPerTick = Mathf.RoundToInt(10000f * (1f - num * 0.2f) * equippedAsset.CatchChallengeEscapeSpeedMultiplier);
                    challengeInputPosition = Mathf.Clamp(fishTargetPosition - equippedAsset.CatchChallengeCursorSize / 2, 0, 10000 - equippedAsset.CatchChallengeCursorSize);
                    challengeInputVelocity = 0;
                    challengeInputWantsToPullUp = true;
                    if (base.channel.IsLocalPlayer)
                    {
                        SetPlayingFishingLoop(playing: true);
                        challengePrizeIcon.Refresh(itemAsset.id, 100, itemAsset.getState(), itemAsset);
                        challengePrizeIcon.SizeOffset_X = itemAsset.size_x * 50;
                        challengePrizeIcon.SizeOffset_Y = itemAsset.size_y * 50;
                        challengePrizeIcon.PositionOffset_X = challengePrizeIcon.SizeOffset_X / -2f;
                        challengePrizeIcon.PositionOffset_Y = challengePrizeIcon.SizeOffset_Y / -2f;
                        challengeWater.SizeOffset_X = challengePrizeIcon.SizeOffset_X + 20f;
                        Color seaColor = LevelLighting.getSeaColor("_BaseColor");
                        seaColor.a = 1f;
                        challengeWater.TintColor = seaColor;
                        challengeProgressBarContainer.PositionOffset_X = challengeWater.PositionOffset_X + challengeWater.SizeOffset_X + 10f;
                        challengeBox.SizeOffset_X = challengeProgressBarContainer.PositionOffset_X + challengeProgressBarContainer.SizeOffset_X + 10f;
                        challengeBox.PositionOffset_X = challengeWater.SizeOffset_X / -2f - 10f;
                        challengeBox.IsVisible = true;
                    }
                }
                else
                {
                    ReelIn();
                }
            }
            else
            {
                if (Provider.isServer && flag)
                {
                    GrantRewards();
                }
                ReelIn();
            }
        }
        else if (fishingState == EFishingState.CatchChallenge)
        {
            challengeInputWantsToPullUp = true;
        }
        return true;
    }

    public override void stopPrimary()
    {
        if (base.player.equipment.isBusy)
        {
            return;
        }
        if (fishingState == EFishingState.PreparingToCast)
        {
            fishingState = EFishingState.LineDeployed;
            if (base.channel.IsLocalPlayer)
            {
                UpdateCastStrengthGaugeVisible(visible: false);
            }
            serverWaterVolume = null;
            base.player.equipment.isBusy = true;
            startedCast = Time.realtimeSinceStartup;
            isPlayingCastAnimation = true;
            if (base.channel.IsLocalPlayer)
            {
                isWaitingForAnimationTrigger = true;
            }
            PlayCastAnimation();
            if (Provider.isServer)
            {
                SendPlayCast.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GatherRemoteClientConnectionsExcludingOwner());
                AlertTool.alert(base.transform.position, 8f);
            }
        }
        else if (fishingState == EFishingState.CatchChallenge)
        {
            challengeInputWantsToPullUp = false;
        }
    }

    public override void equip()
    {
        base.player.animator.play("Equip", smooth: true);
        castAnimationLength = base.player.animator.GetAnimationLength("Cast");
        reelAnimationLength = base.player.animator.GetAnimationLength("Reel");
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
            challengeBox = Glazier.Get().CreateBox();
            challengeBox.PositionOffset_Y = -160f;
            challengeBox.PositionScale_X = 0.5f;
            challengeBox.PositionScale_Y = 0.5f;
            challengeBox.SizeOffset_X = 120f;
            challengeBox.SizeOffset_Y = 320f;
            PlayerLifeUI.container.AddChild(challengeBox);
            challengeBox.IsVisible = false;
            challengeWater = Glazier.Get().CreateImage();
            challengeWater.PositionOffset_X = 10f;
            challengeWater.PositionOffset_Y = 10f;
            challengeWater.SizeOffset_X = 80f;
            challengeWater.SizeOffset_Y = -20f;
            challengeWater.SizeScale_Y = 1f;
            challengeWater.Texture = (Texture2D)GlazierResources.PixelTexture;
            challengeBox.AddChild(challengeWater);
            challengeCursor = Glazier.Get().CreateImage();
            challengeCursor.TintColor = ESleekTint.FOREGROUND;
            challengeCursor.SizeScale_X = 1f;
            challengeCursor.SizeScale_Y = (float)GetEquippedAsset<ItemFisherAsset>().CatchChallengeCursorSize / 10000f;
            challengeCursor.Texture = (Texture2D)GlazierResources.PixelTexture;
            challengeWater.AddChild(challengeCursor);
            challengeProgressBarContainer = Glazier.Get().CreateFrame();
            challengeProgressBarContainer.PositionOffset_X = 100f;
            challengeProgressBarContainer.PositionOffset_Y = 10f;
            challengeProgressBarContainer.SizeOffset_X = 10f;
            challengeProgressBarContainer.SizeOffset_Y = -20f;
            challengeProgressBarContainer.SizeScale_Y = 1f;
            challengeBox.AddChild(challengeProgressBarContainer);
            challengeSuccessBar = Glazier.Get().CreateImage();
            challengeSuccessBar.SizeScale_X = 1f;
            challengeSuccessBar.Texture = (Texture2D)GlazierResources.PixelTexture;
            challengeProgressBarContainer.AddChild(challengeSuccessBar);
            challengeFailureBar = Glazier.Get().CreateImage();
            challengeFailureBar.SizeScale_X = 1f;
            challengeFailureBar.Texture = (Texture2D)GlazierResources.PixelTexture;
            challengeFailureBar.TintColor = ESleekTint.BAD;
            challengeProgressBarContainer.AddChild(challengeFailureBar);
            challengePrizeIcon = new SleekItemIcon();
            challengePrizeIcon.PositionScale_X = 0.5f;
            challengeWater.AddChild(challengePrizeIcon);
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
            if (challengeBox != null)
            {
                PlayerLifeUI.container.RemoveChild(challengeBox);
            }
            SetPlayingFishingLoop(playing: false);
            if (hasClosedMainHud)
            {
                hasClosedMainHud = false;
                PlayerLifeUI.open();
            }
        }
    }

    public override void tock(uint clock)
    {
        if (fishingState == EFishingState.PreparingToCast)
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
        else
        {
            if (fishingState != EFishingState.CatchChallenge)
            {
                return;
            }
            ItemFisherAsset equippedAsset = GetEquippedAsset<ItemFisherAsset>();
            if (ticksUntilFishRelocates > 0)
            {
                ticksUntilFishRelocates--;
            }
            else
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(nextRewardSeed);
                nextRewardSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                ticksUntilFishRelocates = UnityEngine.Random.Range(catchableProperties.minChangeTargetTicks, catchableProperties.maxChangeTargetTicks);
                int num2 = UnityEngine.Random.Range(catchableProperties.minTargetDelta, catchableProperties.maxTargetDelta);
                if (fishTargetPosition + num2 > catchableProperties.maxTargetPosition)
                {
                    fishTargetPosition = Mathf.Max(catchableProperties.minTargetPosition, fishTargetPosition - num2);
                }
                else if (fishTargetPosition - num2 < catchableProperties.minTargetPosition)
                {
                    fishTargetPosition = Mathf.Min(catchableProperties.maxTargetPosition, fishTargetPosition + num2);
                }
                else
                {
                    if (UnityEngine.Random.value < 0.5f)
                    {
                        num2 = -num2;
                    }
                    fishTargetPosition += num2;
                }
                UnityEngine.Random.state = state;
            }
            int value = catchableProperties.springStiffness * (fishTargetPosition - fishPosition) / 10000 - catchableProperties.springDamping * fishVelocity / 10000;
            value = Mathf.Clamp(value, -catchableProperties.maxDownwardAcceleration, catchableProperties.maxUpwardAcceleration);
            fishVelocity += value / 50;
            fishVelocity = Mathf.Clamp(fishVelocity, -catchableProperties.maxDownwardSpeed, catchableProperties.maxUpwardSpeed);
            fishPosition += fishVelocity / 50;
            if (fishPosition > 10000)
            {
                fishPosition = 10000 - (fishPosition - 10000);
                fishVelocity = -fishVelocity * catchableProperties.upperRestitution / 10000;
            }
            else if (fishPosition < 0)
            {
                fishPosition = -fishPosition;
                fishVelocity = -fishVelocity * catchableProperties.lowerRestitution / 10000;
            }
            if (challengeInputWantsToPullUp)
            {
                challengeInputVelocity += equippedAsset.CatchChallengeAcceleration / 50;
            }
            else
            {
                challengeInputVelocity -= equippedAsset.CatchChallengeGravity / 50;
            }
            challengeInputPosition += challengeInputVelocity / 50;
            if (challengeInputPosition + equippedAsset.CatchChallengeCursorSize > 10000)
            {
                challengeInputPosition = 10000 - equippedAsset.CatchChallengeCursorSize - (challengeInputPosition + equippedAsset.CatchChallengeCursorSize - 10000);
                challengeInputVelocity = -challengeInputVelocity * equippedAsset.CatchChallengeUpperRestitution / 10000;
            }
            else if (challengeInputPosition < 0)
            {
                challengeInputPosition = 0;
                challengeInputVelocity = -challengeInputVelocity * equippedAsset.CatchChallengeLowerRestitution / 10000;
            }
            bool flag = fishPosition >= challengeInputPosition && fishPosition <= challengeInputPosition + equippedAsset.CatchChallengeCursorSize;
            if (flag)
            {
                challengeCaptureProgress = Mathf.Min(Mathf.Max(0, challengeCaptureProgress + challengeCaptureProgressPerTick), catchableProperties.captureTicks);
            }
            else
            {
                challengeCaptureProgress = Mathf.Max(challengeCaptureProgress - challengeEscapeProgressPerTick, -catchableProperties.escapeTicks);
            }
            if (challengeCaptureProgress == catchableProperties.captureTicks)
            {
                if (base.channel.IsLocalPlayer)
                {
                    challengeBox.IsVisible = false;
                    SetPlayingFishingLoop(playing: false);
                    PlayFishingSuccess();
                    nextRewardItem.Get<ItemAsset>()?.PlayInventoryAudio2D();
                }
                if (Provider.isServer)
                {
                    GrantRewards();
                }
                ReelIn();
            }
            else if (challengeCaptureProgress == -catchableProperties.escapeTicks)
            {
                base.player.animator.play("Catch_Failure", smooth: false);
                if (base.channel.IsLocalPlayer)
                {
                    challengeBox.IsVisible = false;
                    SetPlayingFishingLoop(playing: false);
                    PlayFishingFailure();
                }
                fishingState = EFishingState.LineDeployed;
                if (Provider.isServer)
                {
                    ResetTimeUntilFishAppears();
                }
            }
            if (base.channel.IsLocalPlayer)
            {
                if (challengePrizeIcon != null)
                {
                    challengePrizeIcon.PositionScale_Y = 1f - (float)fishPosition / 10000f;
                }
                if (challengeCursor != null)
                {
                    challengeCursor.PositionScale_Y = 1f - (float)(challengeInputPosition + equippedAsset.CatchChallengeCursorSize) / 10000f;
                    challengeCursor.TintColor = (flag ? ESleekTint.FOREGROUND : ESleekTint.BAD);
                }
                if (challengeSuccessBar != null)
                {
                    challengeSuccessBar.IsVisible = challengeCaptureProgress > 0;
                    float num3 = (float)challengeCaptureProgress / (float)catchableProperties.captureTicks;
                    challengeSuccessBar.SizeScale_Y = num3;
                    challengeSuccessBar.PositionScale_Y = 1f - num3;
                    challengeSuccessBar.TintColor = ItemTool.getQualityColor(num3);
                }
                if (challengeFailureBar != null)
                {
                    challengeFailureBar.IsVisible = challengeCaptureProgress < 0;
                    float sizeScale_Y = (float)(-challengeCaptureProgress) / (float)catchableProperties.escapeTicks;
                    challengeFailureBar.SizeScale_Y = sizeScale_Y;
                }
            }
        }
    }

    public override void tick()
    {
        if (!base.player.equipment.IsEquipAnimationFinished || !base.channel.IsLocalPlayer)
        {
            return;
        }
        if (isWaitingForAnimationTrigger && HasReachedAnimationTrigger)
        {
            isWaitingForAnimationTrigger = false;
            if (isPlayingCastAnimation)
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
                GameObject original = Assets.coreMasterBundle.LoadAsset<GameObject>("Fishers/Bob.prefab");
                bobberTransform = UnityEngine.Object.Instantiate(original, position, Quaternion.identity).transform;
                bobberTransform.name = "Bob";
                bobberRigidbody = bobberTransform.GetComponent<Rigidbody>();
                if (bobberRigidbody != null)
                {
                    bobberRigidbody.AddForce(forward * Mathf.Lerp(500f, 1000f, strengthMultiplier));
                    bobberRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                }
                isWaitingForBobberToFindWater = true;
            }
            else if (isPlayingReelAnimation && bobberTransform != null)
            {
                UnityEngine.Object.Destroy(bobberTransform.gameObject);
            }
        }
        UpdateLineEndpoints();
        if (bobberTransform != null)
        {
            UpdateBobber();
        }
    }

    public override void simulate(uint simulation, bool inputSteady)
    {
        if (isPlayingCastAnimation && HasFinishedCastAnimation)
        {
            base.player.equipment.isBusy = false;
            isPlayingCastAnimation = false;
        }
        else if (isPlayingReelAnimation && HasFinishedReelAnimation)
        {
            base.player.equipment.isBusy = false;
            isPlayingReelAnimation = false;
        }
        timeSinceFishNotification += PlayerInput.RATE;
        if (!Provider.isServer || fishingState != EFishingState.LineDeployed || !(serverWaterVolume != null))
        {
            return;
        }
        serverTimeUntilFishAppears -= PlayerInput.RATE;
        if (!(serverTimeUntilFishAppears <= 0f))
        {
            return;
        }
        ItemAsset itemAsset;
        if (!serverHasSentFishNotification)
        {
            serverHasSentFishNotification = true;
            timeSinceFishNotification = 0f;
            ItemFisherAsset itemFisherAsset = (ItemFisherAsset)base.player.equipment.asset;
            if (itemFisherAsset.FishingRewardMode == EFishingRewardMode.WaterVolumes)
            {
                LevelAsset asset = Level.getAsset();
                if (asset != null && asset.SupportsFishingVolumes)
                {
                    if (serverWaterVolume != null)
                    {
                        SpawnAsset fishSpawnTable = serverWaterVolume.GetFishSpawnTable();
                        if (fishSpawnTable != null)
                        {
                            itemAsset = SpawnTableTool.Resolve<ItemAsset>(fishSpawnTable, EAssetType.ITEM, serverWaterVolume.OnGetFishErrorContext);
                        }
                        else
                        {
                            fishSpawnTable = Level.getAsset()?.GetDefaultFishingSpawnTable();
                            itemAsset = ((fishSpawnTable == null) ? null : SpawnTableTool.Resolve<ItemAsset>(fishSpawnTable, EAssetType.ITEM, Level.getAsset().OnGetFishErrorContext));
                        }
                    }
                    else
                    {
                        itemAsset = null;
                    }
                    goto IL_0187;
                }
            }
            itemAsset = SpawnTableTool.Resolve<ItemAsset>(itemFisherAsset.rewardID, EAssetType.ITEM, OnGetRewardErrorContext);
            goto IL_0187;
        }
        goto IL_01dc;
        IL_01dc:
        if (timeSinceFishNotification > 5f)
        {
            ResetTimeUntilFishAppears();
        }
        return;
        IL_0187:
        nextRewardItem = itemAsset;
        Guid arg = itemAsset?.GUID ?? Guid.Empty;
        nextRewardSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        SendFishNotification.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), arg, nextRewardSeed);
        goto IL_01dc;
    }

    private void ResetTimeUntilFishAppears()
    {
        serverHasSentFishNotification = false;
        float minInclusive = Provider.modeConfigData?.Gameplay?.Min_Fishing_Bite_Interval ?? 1f;
        float maxInclusive = Provider.modeConfigData?.Gameplay?.Max_Fishing_Bite_Interval ?? 1f;
        float b = Provider.modeConfigData?.Gameplay?.Fishing_MaxStrength_Bite_Interval_Multiplier ?? 1f;
        serverTimeUntilFishAppears = UnityEngine.Random.Range(minInclusive, maxInclusive);
        serverTimeUntilFishAppears *= Mathf.Lerp(1f, b, strengthMultiplier);
        serverTimeUntilFishAppears *= GetEquippedAsset<ItemFisherAsset>().FishBiteIntervalMultiplier;
        serverTimeUntilFishAppears *= LevelLighting.GetFishingBiteIntervalMultiplier(base.player.movement.WeatherMask);
    }

    private void UpdateLineEndpoints()
    {
        if (bobberTransform != null)
        {
            if (base.player.look.perspective == EPlayerPerspective.FIRST)
            {
                Vector3 position = MainCamera.instance.WorldToViewportPoint(bobberTransform.position);
                Vector3 position2 = base.player.animator.viewmodelCamera.ViewportToWorldPoint(position);
                firstLine.SetPosition(0, firstHook.position);
                firstLine.SetPosition(1, position2);
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

    private void UpdateBobber()
    {
        if (isWaitingForBobberToFindWater)
        {
            WaterVolume fishingVolume = VolumeManager<WaterVolume, WaterVolumeManager>.Get().GetFishingVolume(bobberTransform.position);
            bool num = fishingVolume != null;
            float num2 = ((fishingVolume != null) ? WaterUtility.getWaterSurfaceElevation(fishingVolume, bobberTransform.position) : (-1024f));
            float num3 = 4f;
            if (fishingVolume != null && fishingVolume.FishingMinimumDepthOverride > -0.5f)
            {
                num3 = fishingVolume.FishingMinimumDepthOverride;
            }
            if (num && bobberTransform.position.y < num2 - num3)
            {
                bobberRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                bobberRigidbody.useGravity = false;
                bobberRigidbody.isKinematic = true;
                waterSurfacePosition = bobberTransform.position;
                waterSurfacePosition.y = num2;
                isWaitingForBobberToFindWater = false;
                NetId netIdFromInstanceId = fishingVolume.GetNetIdFromInstanceId();
                SendBobberInWaterConfirmation.Invoke(GetNetId(), ENetReliability.Reliable, netIdFromInstanceId);
            }
        }
        else if (timeSinceFishNotification >= 1f && timeSinceFishNotification <= 2.4f)
        {
            if (!hasPlayedTugAnimation)
            {
                hasPlayedTugAnimation = true;
                if (!isPlayingReelAnimation)
                {
                    base.player.playSound(((ItemFisherAsset)base.player.equipment.asset).tug);
                    base.player.animator.play("Tug", smooth: false);
                }
            }
            bobberRigidbody.MovePosition(Vector3.Lerp(bobberTransform.position, waterSurfacePosition + Vector3.down * 4f + Vector3.left * UnityEngine.Random.Range(-4f, 4f) + Vector3.forward * UnityEngine.Random.Range(-4f, 4f), 4f * Time.deltaTime));
        }
        else
        {
            bobberRigidbody.MovePosition(Vector3.Lerp(bobberTransform.position, waterSurfacePosition + Vector3.up * Mathf.Sin(Time.time) * 0.25f, 4f * Time.deltaTime));
        }
    }

    private void ReelIn()
    {
        fishingState = EFishingState.Idle;
        base.player.equipment.isBusy = true;
        startedReel = Time.realtimeSinceStartup;
        isPlayingReelAnimation = true;
        if (base.channel.IsLocalPlayer)
        {
            isWaitingForAnimationTrigger = true;
        }
        PlayReelAnimation();
        if (Provider.isServer)
        {
            SendPlayReel.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GatherRemoteClientConnectionsExcludingOwner());
            AlertTool.alert(base.transform.position, 8f);
        }
    }

    private void GrantRewards()
    {
        ItemAsset itemAsset = nextRewardItem.Get<ItemAsset>();
        if (itemAsset != null)
        {
            base.player.inventory.forceAddItem(new Item(itemAsset, EItemOrigin.NATURE), auto: false);
        }
        base.player.sendStat(EPlayerStat.FOUND_FISHES);
        ItemFisherAsset equippedAsset = GetEquippedAsset<ItemFisherAsset>();
        int num = UnityEngine.Random.Range(equippedAsset.rewardExperienceMin, equippedAsset.rewardExperienceMax + 1);
        if (num > 0)
        {
            base.player.skills.askPay((uint)num);
        }
        equippedAsset.rewardsList.Grant(base.player);
    }

    private string OnGetRewardErrorContext()
    {
        return "fishing " + base.player.equipment.asset?.FriendlyName + " reward";
    }

    [Conditional("LOG_FISHING_CATCH_CHALLENGE")]
    private void LogCatchChallenge(object text)
    {
        CommandWindow.Log($"[Fishing Catch Challenge]: {text}");
    }
}
