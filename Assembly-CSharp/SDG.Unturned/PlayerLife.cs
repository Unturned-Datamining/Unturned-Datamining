using System;
using System.Collections.Generic;
using SDG.Framework.Devkit;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerLife : PlayerCaller
{
    public delegate void PlayerDiedCallback(PlayerLife sender, EDeathCause cause, ELimb limb, CSteamID instigator);

    public delegate void RespawnPointSelector(PlayerLife sender, bool wantsToSpawnAtHome, ref Vector3 position, ref float yaw);

    public delegate void FallDamageRequestHandler(PlayerLife component, float velocity, ref float damage, ref bool shouldBreakLegs);

    public static readonly byte SAVEDATA_VERSION_LATEST = 3;

    public static readonly byte SAVEDATA_VERSION_WITH_OXYGEN = 3;

    [Obsolete("Future version numbers for all systems will specify what changed.")]
    public static readonly byte SAVEDATA_VERSION = SAVEDATA_VERSION_LATEST;

    private static readonly float COMBAT_COOLDOWN = 30f;

    public static PlayerLifeUpdated onPlayerLifeUpdated;

    public static Action<PlayerLife> OnTellHealth_Global;

    public static Action<PlayerLife> OnTellFood_Global;

    public static Action<PlayerLife> OnTellWater_Global;

    public static Action<PlayerLife> OnTellVirus_Global;

    public static Action<PlayerLife> OnTellBleeding_Global;

    public static Action<PlayerLife> OnTellBroken_Global;

    public static Action<PlayerLife, EDeathCause, ELimb, CSteamID> RocketLegacyOnDeath;

    /// <summary>
    /// Invoked after player finishes respawning.
    /// </summary>
    public static Action<PlayerLife> OnRevived_Global;

    public LifeUpdated onLifeUpdated;

    public HealthUpdated onHealthUpdated;

    public FoodUpdated onFoodUpdated;

    public WaterUpdated onWaterUpdated;

    public VirusUpdated onVirusUpdated;

    public StaminaUpdated onStaminaUpdated;

    public VisionUpdated onVisionUpdated;

    public OxygenUpdated onOxygenUpdated;

    public BleedingUpdated onBleedingUpdated;

    public BrokenUpdated onBrokenUpdated;

    public TemperatureUpdated onTemperatureUpdated;

    public Damaged onDamaged;

    private static EDeathCause _deathCause;

    private static ELimb _deathLimb;

    private static CSteamID _deathKiller;

    private CSteamID recentKiller;

    private float lastTimeAggressive;

    private float lastTimeTookDamage;

    private float lastTimeCausedDamage;

    private bool _isDead;

    private byte lastHealth;

    private byte _health;

    private byte _food;

    private byte _water;

    private byte _virus;

    private byte _vision;

    private uint _warmth;

    private byte _stamina;

    private byte _oxygen;

    private bool _isBleeding;

    private bool _isBroken;

    private EPlayerTemperature _temperature;

    private uint lastStarve;

    private uint lastDehydrate;

    private uint lastUncleaned;

    private uint lastView;

    internal uint lastTire;

    private uint lastSuffocate;

    internal uint lastRest;

    private uint lastBreath;

    private uint lastInfect;

    private uint lastBleed;

    private uint lastBleeding;

    private uint lastBroken;

    private uint lastFreeze;

    private uint lastWarm;

    private uint lastBurn;

    private uint lastCovered;

    private uint lastRegenerate;

    private uint lastRadiate;

    private uint lastOutsideDeadzoneFrame;

    private float pendingDeadzoneDamage;

    private float pendingDeadzoneRadiation;

    private float pendingDeadzoneMaskFilterQualityLoss;

    private bool wasWarm;

    private bool wasCovered;

    private float _lastRespawn = -1f;

    private float _lastDeath;

    private float lastSuicide;

    private float lastAlive;

    private Vector3 ragdoll;

    private ERagdollEffect ragdollEffect;

    private PlayerSpawnpoint spawnpoint;

    private static readonly ClientInstanceMethod<EDeathCause, ELimb, CSteamID> SendDeath = ClientInstanceMethod<EDeathCause, ELimb, CSteamID>.Get(typeof(PlayerLife), "ReceiveDeath");

    private static readonly ClientInstanceMethod<Vector3, ERagdollEffect> SendDead = ClientInstanceMethod<Vector3, ERagdollEffect>.Get(typeof(PlayerLife), "ReceiveDead");

    private static readonly ClientInstanceMethod<Vector3, byte> SendRevive = ClientInstanceMethod<Vector3, byte>.Get(typeof(PlayerLife), "ReceiveRevive");

    private static readonly ClientInstanceMethod<byte, byte, byte, byte, byte, bool, bool> SendLifeStats = ClientInstanceMethod<byte, byte, byte, byte, byte, bool, bool>.Get(typeof(PlayerLife), "ReceiveLifeStats");

    private static readonly ClientInstanceMethod<byte> SendHealth = ClientInstanceMethod<byte>.Get(typeof(PlayerLife), "ReceiveHealth");

    private static readonly ClientInstanceMethod<byte, Vector3> SendDamagedEvent = ClientInstanceMethod<byte, Vector3>.Get(typeof(PlayerLife), "ReceiveDamagedEvent");

    private static readonly ClientInstanceMethod<byte> SendFood = ClientInstanceMethod<byte>.Get(typeof(PlayerLife), "ReceiveFood");

    private static readonly ClientInstanceMethod<byte> SendWater = ClientInstanceMethod<byte>.Get(typeof(PlayerLife), "ReceiveWater");

    private static readonly ClientInstanceMethod<byte> SendVirus = ClientInstanceMethod<byte>.Get(typeof(PlayerLife), "ReceiveVirus");

    private static readonly ClientInstanceMethod<bool> SendBleeding = ClientInstanceMethod<bool>.Get(typeof(PlayerLife), "ReceiveBleeding");

    private static readonly ClientInstanceMethod<bool> SendBroken = ClientInstanceMethod<bool>.Get(typeof(PlayerLife), "ReceiveBroken");

    private static readonly ClientInstanceMethod<short> SendModifyStamina = ClientInstanceMethod<short>.Get(typeof(PlayerLife), "ReceiveModifyStamina");

    private static readonly ClientInstanceMethod<short> SendModifyHallucination = ClientInstanceMethod<short>.Get(typeof(PlayerLife), "ReceiveModifyHallucination");

    private static readonly ClientInstanceMethod<short> SendModifyWarmth = ClientInstanceMethod<short>.Get(typeof(PlayerLife), "ReceiveModifyWarmth");

    private static readonly ServerInstanceMethod<bool> SendRespawnRequest = ServerInstanceMethod<bool>.Get(typeof(PlayerLife), "ReceiveRespawnRequest");

    private static readonly ServerInstanceMethod SendSuicideRequest = ServerInstanceMethod.Get(typeof(PlayerLife), "ReceiveSuicideRequest");

    /// <summary>
    /// Used by UI. True when underwater or inside non-breathable oxygen volume.
    /// </summary>
    internal bool isAsphyxiating;

    private static readonly AssetReference<EffectAsset> BonesRef = new AssetReference<EffectAsset>("663158e0a71346068947b29978818ef7");

    private bool wasLoadCalled;

    public bool wasPvPDeath { get; private set; }

    public static EDeathCause deathCause => _deathCause;

    public static ELimb deathLimb => _deathLimb;

    public static CSteamID deathKiller => _deathKiller;

    public bool isAggressor => Time.realtimeSinceStartup - lastTimeAggressive < COMBAT_COOLDOWN;

    public bool isDead => _isDead;

    public bool IsAlive => !_isDead;

    public byte health => _health;

    public byte food => _food;

    public byte water => _water;

    public byte virus => _virus;

    public byte vision => _vision;

    public uint warmth => _warmth;

    public byte stamina => _stamina;

    public byte oxygen => _oxygen;

    public bool isBleeding => _isBleeding;

    public bool isBroken => _isBroken;

    public EPlayerTemperature temperature => _temperature;

    public float lastRespawn => _lastRespawn;

    public float lastDeath => _lastDeath;

    /// <summary>
    /// Invoked prior to built-in death logic.
    /// </summary>
    public static event Action<PlayerLife> OnPreDeath;

    /// <summary>
    /// Event for plugins when player dies.
    /// </summary>
    public static event PlayerDiedCallback onPlayerDied;

    public static event RespawnPointSelector OnSelectingRespawnPoint;

    public event Hurt onHurt;

    internal event System.Action OnIsAsphyxiatingChanged;

    public event FallDamageRequestHandler OnFallDamageRequested;

    private static void broadcastPlayerDied(PlayerLife sender, EDeathCause cause, ELimb limb, CSteamID instigator)
    {
        try
        {
            PlayerLife.onPlayerDied?.Invoke(sender, cause, limb, instigator);
        }
        catch (Exception e)
        {
            UnturnedLog.warn("Plugin raised an exception from onPlayerDied:");
            UnturnedLog.exception(e);
        }
    }

    /// <summary>
    /// Tracks this player as an aggressor if they were recently an aggressor or if they haven't been attacked recently.
    /// </summary>
    /// <param name="force">Ignores rules and just make aggressive.</param>
    /// <param name="spreadToGroup">Whether to call markAggressive on group members.</param>
    public void markAggressive(bool force, bool spreadToGroup = true)
    {
        if (force || Time.realtimeSinceStartup - lastTimeAggressive < COMBAT_COOLDOWN)
        {
            lastTimeAggressive = Time.realtimeSinceStartup;
        }
        else if (recentKiller == CSteamID.Nil || Time.realtimeSinceStartup - lastTimeTookDamage > COMBAT_COOLDOWN)
        {
            lastTimeAggressive = Time.realtimeSinceStartup;
        }
        if (!spreadToGroup || !base.player.quests.isMemberOfAGroup)
        {
            return;
        }
        for (int i = 0; i < Provider.clients.Count; i++)
        {
            if (Provider.clients[i].playerID.steamID != base.channel.owner.playerID.steamID && base.player.quests.isMemberOfSameGroupAs(Provider.clients[i].player) && Provider.clients[i].player != null)
            {
                Provider.clients[i].player.life.markAggressive(force, spreadToGroup: false);
            }
        }
    }

    [Obsolete]
    public void tellDeath(CSteamID steamID, byte newCause, byte newLimb, CSteamID newKiller)
    {
        ReceiveDeath((EDeathCause)newCause, (ELimb)newLimb, newKiller);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellDeath")]
    public void ReceiveDeath(EDeathCause newCause, ELimb newLimb, CSteamID newKiller)
    {
        _deathCause = newCause;
        _deathLimb = newLimb;
        _deathKiller = newKiller;
        if (base.channel.IsLocalPlayer && Provider.provider.statisticsService.userStatisticsService.getStatistic("Deaths_Players", out int data))
        {
            Provider.provider.statisticsService.userStatisticsService.setStatistic("Deaths_Players", data + 1);
        }
    }

    [Obsolete]
    public void tellDead(CSteamID steamID, Vector3 newRagdoll, byte newRagdollEffect)
    {
        ReceiveDead(newRagdoll, (ERagdollEffect)newRagdollEffect);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellDead")]
    public void ReceiveDead(Vector3 newRagdoll, ERagdollEffect newRagdollEffect)
    {
        _isDead = true;
        _lastDeath = Time.realtimeSinceStartup;
        ragdoll = newRagdoll;
        ragdollEffect = newRagdollEffect;
        if (!Dedicator.IsDedicatedServer)
        {
            RagdollTool.ragdollPlayer(base.transform.position, base.transform.rotation, base.player.animator.thirdSkeleton, ragdoll, base.player.clothing, ragdollEffect);
        }
        if (base.channel.IsLocalPlayer || Provider.isServer)
        {
            base.player.movement.UpdateCharacterControllerEnabled();
        }
        if (onLifeUpdated != null)
        {
            onLifeUpdated(isDead);
        }
        if (onPlayerLifeUpdated != null)
        {
            onPlayerLifeUpdated(base.player);
        }
    }

    [Obsolete]
    public void tellRevive(CSteamID steamID, Vector3 position, byte angle)
    {
        ReceiveRevive(position, angle);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellRevive")]
    public void ReceiveRevive(Vector3 position, byte angle)
    {
        _isDead = false;
        _lastRespawn = Time.realtimeSinceStartup;
        base.player.ReceiveTeleport(position, angle);
        if (base.channel.IsLocalPlayer || Provider.isServer)
        {
            base.player.movement.UpdateCharacterControllerEnabled();
        }
        onLifeUpdated?.Invoke(isDead);
        onPlayerLifeUpdated?.Invoke(base.player);
        try
        {
            OnRevived_Global?.Invoke(this);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Plugin threw an exception during OnRevived_Global:");
        }
    }

    [Obsolete("Prior to saving/loading oxygen the client assumed it started at 100, but now needs the exact value.")]
    public void tellLife(CSteamID steamID, byte newHealth, byte newFood, byte newWater, byte newVirus, bool newBleeding, bool newBroken)
    {
        tellLifeWithOxygen(steamID, newHealth, newFood, newWater, newVirus, 100, newBleeding, newBroken);
    }

    [Obsolete]
    public void tellLifeWithOxygen(CSteamID steamID, byte newHealth, byte newFood, byte newWater, byte newVirus, byte newOxygen, bool newBleeding, bool newBroken)
    {
        ReceiveLifeStats(newHealth, newFood, newWater, newVirus, newOxygen, newBleeding, newBroken);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellLifeWithOxygen")]
    public void ReceiveLifeStats(byte newHealth, byte newFood, byte newWater, byte newVirus, byte newOxygen, bool newBleeding, bool newBroken)
    {
        Player.isLoadingLife = false;
        ReceiveHealth(newHealth);
        ReceiveFood(newFood);
        ReceiveWater(newWater);
        ReceiveVirus(newVirus);
        ReceiveBleeding(newBleeding);
        ReceiveBroken(newBroken);
        _stamina = 100;
        _oxygen = newOxygen;
        _vision = 0;
        _warmth = 0u;
        _temperature = EPlayerTemperature.NONE;
        wasWarm = false;
        wasCovered = false;
        onVisionUpdated?.Invoke(isViewing: false);
        onStaminaUpdated?.Invoke(stamina);
        onOxygenUpdated?.Invoke(oxygen);
        onTemperatureUpdated?.Invoke(temperature);
        lastAlive = Time.realtimeSinceStartup;
    }

    [Obsolete]
    public void askLife(CSteamID steamID)
    {
    }

    internal void SendInitialPlayerState(SteamPlayer client)
    {
        if (base.channel.owner == client)
        {
            SendLifeStats.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, health, food, water, virus, oxygen, isBleeding, isBroken);
        }
        else if (isDead)
        {
            SendDead.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, ragdoll, ragdollEffect);
        }
    }

    internal void SendInitialPlayerState(List<ITransportConnection> transportConnections)
    {
        if (isDead)
        {
            SendDead.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, ragdoll, ragdollEffect);
        }
    }

    [Obsolete]
    public void tellHealth(CSteamID steamID, byte newHealth)
    {
        ReceiveHealth(newHealth);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellHealth")]
    public void ReceiveHealth(byte newHealth)
    {
        _health = newHealth;
        onHealthUpdated?.Invoke(health);
        if (newHealth < lastHealth - 3)
        {
            onDamaged?.Invoke((byte)(lastHealth - newHealth));
        }
        lastHealth = newHealth;
        OnTellHealth_Global?.Invoke(this);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveDamagedEvent(byte damageAmount, Vector3 damageDirection)
    {
        base.player.look.FlinchFromDamage(damageAmount, damageDirection);
    }

    [Obsolete]
    public void tellFood(CSteamID steamID, byte newFood)
    {
        ReceiveFood(newFood);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellFood")]
    public void ReceiveFood(byte newFood)
    {
        _food = newFood;
        onFoodUpdated?.Invoke(food);
        OnTellFood_Global?.Invoke(this);
    }

    [Obsolete]
    public void tellWater(CSteamID steamID, byte newWater)
    {
        ReceiveWater(newWater);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellWater")]
    public void ReceiveWater(byte newWater)
    {
        _water = newWater;
        onWaterUpdated?.Invoke(water);
        OnTellWater_Global?.Invoke(this);
    }

    [Obsolete]
    public void tellVirus(CSteamID steamID, byte newVirus)
    {
        ReceiveVirus(newVirus);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVirus")]
    public void ReceiveVirus(byte newVirus)
    {
        _virus = newVirus;
        onVirusUpdated?.Invoke(virus);
        OnTellVirus_Global?.Invoke(this);
    }

    [Obsolete]
    public void tellBleeding(CSteamID steamID, bool newBleeding)
    {
        ReceiveBleeding(newBleeding);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellBleeding")]
    public void ReceiveBleeding(bool newBleeding)
    {
        _isBleeding = newBleeding;
        onBleedingUpdated?.Invoke(isBleeding);
        OnTellBleeding_Global?.Invoke(this);
    }

    [Obsolete]
    public void tellBroken(CSteamID steamID, bool newBroken)
    {
        ReceiveBroken(newBroken);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellBroken")]
    public void ReceiveBroken(bool newBroken)
    {
        _isBroken = newBroken;
        onBrokenUpdated?.Invoke(isBroken);
        OnTellBroken_Global?.Invoke(this);
    }

    public void askDamage(byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill)
    {
        askDamage(amount, newRagdoll, newCause, newLimb, newKiller, out kill, trackKill: false, ERagdollEffect.NONE, canCauseBleeding: true);
    }

    public void askDamage(byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE)
    {
        askDamage(amount, newRagdoll, newCause, newLimb, newKiller, out kill, trackKill, newRagdollEffect, canCauseBleeding: true);
    }

    public void askDamage(byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
    {
        askDamage(amount, newRagdoll, newCause, newLimb, newKiller, out kill, trackKill, newRagdollEffect, canCauseBleeding, bypassSafezone: false);
    }

    /// <param name="bypassSafezone">Should damage be dealt even while inside safezone?</param>
    public void askDamage(byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true, bool bypassSafezone = false)
    {
        kill = EPlayerKill.NONE;
        if (bypassSafezone || InternalCanDamage())
        {
            doDamage(amount, newRagdoll, newCause, newLimb, newKiller, out kill, trackKill, newRagdollEffect, canCauseBleeding);
        }
    }

    internal bool InternalCanDamage()
    {
        if (base.player.movement.isSafe && base.player.movement.isSafeInfo.noWeapons)
        {
            return false;
        }
        if (lastRespawn > 0f && Time.realtimeSinceStartup - lastRespawn < 0.5f)
        {
            return false;
        }
        return true;
    }

    private void doDamage(byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
    {
        kill = EPlayerKill.NONE;
        if (amount == 0 || isDead || !IsAlive)
        {
            return;
        }
        if (amount >= health)
        {
            _health = 0;
        }
        else
        {
            _health -= amount;
        }
        ragdoll = newRagdoll;
        ragdollEffect = newRagdollEffect;
        if (_health > 0 && amount > 3)
        {
            SendDamagedEvent.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), amount, newRagdoll.normalized);
        }
        SendHealth.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), health);
        OnTellHealth_Global?.Invoke(this);
        if (newCause == EDeathCause.GUN || newCause == EDeathCause.MELEE || newCause == EDeathCause.PUNCH || newCause == EDeathCause.ROADKILL || newCause == EDeathCause.GRENADE || newCause == EDeathCause.MISSILE || newCause == EDeathCause.CHARGE)
        {
            recentKiller = newKiller;
            lastTimeTookDamage = Time.realtimeSinceStartup;
            Player player = PlayerTool.getPlayer(recentKiller);
            if (player != null)
            {
                player.life.lastTimeCausedDamage = Time.realtimeSinceStartup;
                if (Time.realtimeSinceStartup - player.life.lastTimeAggressive < COMBAT_COOLDOWN)
                {
                    player.life.markAggressive(force: true);
                }
                else if ((player.life.recentKiller == CSteamID.Nil || Time.realtimeSinceStartup - player.life.lastTimeTookDamage > COMBAT_COOLDOWN) && Time.realtimeSinceStartup - lastTimeCausedDamage > COMBAT_COOLDOWN)
                {
                    player.life.markAggressive(force: true);
                }
            }
        }
        if (health == 0)
        {
            try
            {
                base.player.quests.InterruptDelayedQuestRewards(EDelayedQuestRewardsInterruption.Death);
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, "Caught exception interrupting delayed quest rewards on death:");
            }
            if (recentKiller != CSteamID.Nil && recentKiller != base.channel.owner.playerID.steamID && Time.realtimeSinceStartup - lastTimeTookDamage < COMBAT_COOLDOWN)
            {
                Player player2 = PlayerTool.getPlayer(recentKiller);
                if (player2 != null)
                {
                    int value = Mathf.Abs(base.player.skills.reputation);
                    value = Mathf.Clamp(value, 1, 25);
                    if (player2.life.isAggressor)
                    {
                        value = -value;
                    }
                    player2.skills.askRep(value);
                }
            }
            kill = EPlayerKill.PLAYER;
            wasPvPDeath = newCause == EDeathCause.GUN || newCause == EDeathCause.MELEE || newCause == EDeathCause.PUNCH || newCause == EDeathCause.ROADKILL || newCause == EDeathCause.GRENADE || newCause == EDeathCause.MISSILE || newCause == EDeathCause.CHARGE || newCause == EDeathCause.SENTRY;
            PlayerLife.OnPreDeath.TryInvoke("OnPreDeath", this);
            base.player.movement.forceRemoveFromVehicle();
            RocketLegacyOnDeath.TryInvoke("RocketLegacyOnDeath", this, newCause, newLimb, newKiller);
            try
            {
                SendDeath.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), newCause, newLimb, newKiller);
                SendDead.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), ragdoll, ragdollEffect);
            }
            catch (Exception e2)
            {
                UnturnedLog.warn("Exception during tellDeath or tellDead:");
                UnturnedLog.exception(e2);
            }
            if (spawnpoint == null || (newCause != EDeathCause.SUICIDE && newCause != EDeathCause.BREATH) || Time.realtimeSinceStartup - lastSuicide > 60f)
            {
                spawnpoint = LevelPlayers.getSpawn(isAlt: false);
            }
            if (newCause == EDeathCause.SUICIDE || newCause == EDeathCause.BREATH)
            {
                lastSuicide = Time.realtimeSinceStartup;
            }
            if (trackKill)
            {
                for (int i = 0; i < Provider.clients.Count; i++)
                {
                    SteamPlayer steamPlayer = Provider.clients[i];
                    if (!(steamPlayer.player == null) && !(steamPlayer.player.movement == null) && !(steamPlayer.player.life == null) && !steamPlayer.player.life.isDead && steamPlayer != base.channel.owner && (steamPlayer.player.transform.position - base.transform.position).sqrMagnitude < 90000f)
                    {
                        steamPlayer.player.quests.trackPlayerKill(base.player);
                    }
                }
            }
            broadcastPlayerDied(this, newCause, newLimb, newKiller);
            if (CommandWindow.shouldLogDeaths)
            {
                switch (newCause)
                {
                case EDeathCause.BLEEDING:
                    CommandWindow.Log(Provider.localization.format("Bleeding", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                    break;
                case EDeathCause.BONES:
                    CommandWindow.Log(Provider.localization.format("Bones", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                    break;
                case EDeathCause.FREEZING:
                    CommandWindow.Log(Provider.localization.format("Freezing", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                    break;
                case EDeathCause.BURNING:
                    CommandWindow.Log(Provider.localization.format("Burning", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                    break;
                case EDeathCause.FOOD:
                    CommandWindow.Log(Provider.localization.format("Food", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                    break;
                case EDeathCause.WATER:
                    CommandWindow.Log(Provider.localization.format("Water", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                    break;
                case EDeathCause.GUN:
                case EDeathCause.MELEE:
                case EDeathCause.PUNCH:
                case EDeathCause.ROADKILL:
                case EDeathCause.GRENADE:
                case EDeathCause.MISSILE:
                case EDeathCause.CHARGE:
                case EDeathCause.SPLASH:
                {
                    SteamPlayer steamPlayer2 = PlayerTool.getSteamPlayer(newKiller);
                    string text;
                    string text2;
                    if (steamPlayer2 != null)
                    {
                        text = steamPlayer2.playerID.characterName;
                        text2 = steamPlayer2.playerID.playerName;
                    }
                    else
                    {
                        text = "?";
                        text2 = "?";
                    }
                    string text3 = "";
                    switch (newLimb)
                    {
                    case ELimb.LEFT_FOOT:
                    case ELimb.LEFT_LEG:
                    case ELimb.RIGHT_FOOT:
                    case ELimb.RIGHT_LEG:
                        text3 = Provider.localization.format("Leg");
                        break;
                    case ELimb.LEFT_HAND:
                    case ELimb.LEFT_ARM:
                    case ELimb.RIGHT_HAND:
                    case ELimb.RIGHT_ARM:
                        text3 = Provider.localization.format("Arm");
                        break;
                    case ELimb.SPINE:
                        text3 = Provider.localization.format("Spine");
                        break;
                    case ELimb.SKULL:
                        text3 = Provider.localization.format("Skull");
                        break;
                    }
                    switch (newCause)
                    {
                    case EDeathCause.GUN:
                        CommandWindow.Log(Provider.localization.format("Gun", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName, text3, text, text2));
                        break;
                    case EDeathCause.MELEE:
                        CommandWindow.Log(Provider.localization.format("Melee", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName, text3, text, text2));
                        break;
                    case EDeathCause.PUNCH:
                        CommandWindow.Log(Provider.localization.format("Punch", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName, text3, text, text2));
                        break;
                    case EDeathCause.ROADKILL:
                        CommandWindow.Log(Provider.localization.format("Roadkill", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName, text, text2));
                        break;
                    case EDeathCause.GRENADE:
                        CommandWindow.Log(Provider.localization.format("Grenade", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName, text, text2));
                        break;
                    case EDeathCause.MISSILE:
                        CommandWindow.Log(Provider.localization.format("Missile", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName, text, text2));
                        break;
                    case EDeathCause.CHARGE:
                        CommandWindow.Log(Provider.localization.format("Charge", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName, text, text2));
                        break;
                    case EDeathCause.SPLASH:
                        CommandWindow.Log(Provider.localization.format("Splash", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName, text, text2));
                        break;
                    }
                    break;
                }
                case EDeathCause.ZOMBIE:
                    CommandWindow.Log(Provider.localization.format("Zombie", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                    break;
                case EDeathCause.ANIMAL:
                    CommandWindow.Log(Provider.localization.format("Animal", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                    break;
                case EDeathCause.SUICIDE:
                    CommandWindow.Log(Provider.localization.format("Suicide", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                    break;
                case EDeathCause.INFECTION:
                    CommandWindow.Log(Provider.localization.format("Infection", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                    break;
                case EDeathCause.BREATH:
                    CommandWindow.Log(Provider.localization.format("Breath", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                    break;
                default:
                    switch (newCause)
                    {
                    case EDeathCause.ZOMBIE:
                        CommandWindow.Log(Provider.localization.format("Zombie", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                        break;
                    case EDeathCause.VEHICLE:
                        CommandWindow.Log(Provider.localization.format("Vehicle", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                        break;
                    case EDeathCause.SHRED:
                        CommandWindow.Log(Provider.localization.format("Shred", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                        break;
                    case EDeathCause.LANDMINE:
                        CommandWindow.Log(Provider.localization.format("Landmine", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                        break;
                    case EDeathCause.ARENA:
                        CommandWindow.Log(Provider.localization.format("Arena", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                        break;
                    case EDeathCause.SENTRY:
                        CommandWindow.Log(Provider.localization.format("Sentry", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                        break;
                    case EDeathCause.ACID:
                        CommandWindow.Log(Provider.localization.format("Acid", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                        break;
                    case EDeathCause.BOULDER:
                        CommandWindow.Log(Provider.localization.format("Boulder", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                        break;
                    case EDeathCause.BURNER:
                        CommandWindow.Log(Provider.localization.format("Burner", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                        break;
                    case EDeathCause.SPIT:
                        CommandWindow.Log(Provider.localization.format("Spit", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                        break;
                    case EDeathCause.SPARK:
                        CommandWindow.Log(Provider.localization.format("Spark", base.channel.owner.playerID.characterName, base.channel.owner.playerID.playerName));
                        break;
                    }
                    break;
                }
            }
        }
        else if (Provider.modeConfigData.Players.Can_Start_Bleeding && canCauseBleeding && amount >= 20)
        {
            serverSetBleeding(newBleeding: true);
        }
        this.onHurt?.Invoke(base.player, amount, newRagdoll, newCause, newLimb, newKiller);
    }

    public void askHeal(byte amount, bool healBleeding, bool healBroken)
    {
        if (amount != 0 && !isDead && IsAlive)
        {
            if (amount >= 100 - health)
            {
                _health = 100;
            }
            else
            {
                _health += amount;
            }
            SendHealth.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), health);
            OnTellHealth_Global?.Invoke(this);
            if (isBleeding && healBleeding)
            {
                serverSetBleeding(newBleeding: false);
            }
            if (isBroken && healBroken)
            {
                serverSetLegsBroken(newLegsBroken: false);
            }
        }
    }

    /// <summary>
    /// Set bleeding state and replicate to owner if changed.
    /// </summary>
    public void serverSetBleeding(bool newBleeding)
    {
        if (newBleeding)
        {
            lastBleeding = base.player.input.simulation;
            lastBleed = base.player.input.simulation;
        }
        if (isBleeding != newBleeding)
        {
            _isBleeding = newBleeding;
            SendBleeding.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), isBleeding);
            OnTellBleeding_Global?.Invoke(this);
        }
    }

    /// <summary>
    /// Set legs broken state and replicate to owner if changed.
    /// </summary>
    public void serverSetLegsBroken(bool newLegsBroken)
    {
        if (newLegsBroken)
        {
            lastBroken = base.player.input.simulation;
        }
        if (isBroken != newLegsBroken)
        {
            _isBroken = newLegsBroken;
            SendBroken.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), isBroken);
            OnTellBroken_Global?.Invoke(this);
        }
    }

    public void askStarve(byte amount)
    {
        if (amount != 0 && !isDead && IsAlive)
        {
            if (amount >= food)
            {
                _food = 0;
            }
            else
            {
                _food -= amount;
            }
            SendFood.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), food);
            OnTellFood_Global?.Invoke(this);
        }
    }

    public void askEat(byte amount)
    {
        if (amount != 0 && !isDead && IsAlive)
        {
            if (amount >= 100 - food)
            {
                _food = 100;
            }
            else
            {
                _food += amount;
            }
            SendFood.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), food);
            OnTellFood_Global?.Invoke(this);
        }
    }

    public void askDehydrate(byte amount)
    {
        if (amount != 0 && !isDead && IsAlive)
        {
            if (amount >= water)
            {
                _water = 0;
            }
            else
            {
                _water -= amount;
            }
            SendWater.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), water);
            OnTellWater_Global?.Invoke(this);
        }
    }

    public void askDrink(byte amount)
    {
        if (amount != 0 && !isDead && IsAlive)
        {
            if (amount >= 100 - water)
            {
                _water = 100;
            }
            else
            {
                _water += amount;
            }
            SendWater.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), water);
            OnTellWater_Global?.Invoke(this);
        }
    }

    public void askInfect(byte amount)
    {
        if (amount != 0 && !isDead && IsAlive)
        {
            if (amount >= virus)
            {
                _virus = 0;
            }
            else
            {
                _virus -= amount;
            }
            SendVirus.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), virus);
            OnTellVirus_Global?.Invoke(this);
        }
    }

    public void askRadiate(byte amount)
    {
        if (amount != 0 && !isDead && IsAlive)
        {
            if (amount >= virus)
            {
                _virus = 0;
            }
            else
            {
                _virus -= amount;
            }
            onVirusUpdated?.Invoke(virus);
            OnTellVirus_Global?.Invoke(this);
        }
    }

    public void askDisinfect(byte amount)
    {
        if (amount != 0 && !isDead && IsAlive)
        {
            if (amount >= 100 - virus)
            {
                _virus = 100;
            }
            else
            {
                _virus += amount;
            }
            SendVirus.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), virus);
            OnTellVirus_Global?.Invoke(this);
        }
    }

    internal void internalSetStamina(byte value)
    {
        _stamina = value;
        onStaminaUpdated?.Invoke(stamina);
    }

    public void askTire(byte amount)
    {
        if (amount != 0 && !isDead && IsAlive)
        {
            lastTire = base.player.input.simulation;
            if (amount >= stamina)
            {
                _stamina = 0;
            }
            else
            {
                _stamina -= amount;
            }
            onStaminaUpdated?.Invoke(stamina);
        }
    }

    public void askRest(byte amount)
    {
        if (amount != 0 && !isDead && IsAlive)
        {
            if (amount >= 100 - stamina)
            {
                _stamina = 100;
            }
            else
            {
                _stamina += amount;
            }
            onStaminaUpdated?.Invoke(stamina);
        }
    }

    /// <summary>
    /// Add to or subtract from stamina level.
    /// Does not replicate the change.
    /// </summary>
    public void simulatedModifyStamina(short delta)
    {
        if (delta > 0)
        {
            askRest((byte)delta);
        }
        else if (delta < 0)
        {
            askTire((byte)(-delta));
        }
    }

    /// <summary>
    /// Add to or subtract from stamina level.
    /// Does not replicate the change.
    /// </summary>
    public void simulatedModifyStamina(float delta)
    {
        simulatedModifyStamina(MathfEx.RoundAndClampToShort(delta));
    }

    [Obsolete]
    public void clientModifyStamina(CSteamID senderId, short delta)
    {
        ReceiveModifyStamina(delta);
    }

    /// <summary>
    /// Called from the server to modify stamina.
    /// </summary>
    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "clientModifyStamina")]
    public void ReceiveModifyStamina(short delta)
    {
        simulatedModifyStamina(delta);
    }

    /// <summary>
    /// Add to or subtract from stamina level on the client and server.
    /// </summary>
    public void serverModifyStamina(float delta)
    {
        short num = MathfEx.RoundAndClampToShort(delta);
        if (num != 0)
        {
            simulatedModifyStamina(num);
            if (!base.channel.IsLocalPlayer)
            {
                SendModifyStamina.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), num);
            }
        }
    }

    public void askView(byte amount)
    {
        if (amount != 0 && !isDead && IsAlive)
        {
            lastView = base.player.input.simulation;
            _vision = amount;
            onVisionUpdated?.Invoke(isViewing: true);
        }
    }

    [Obsolete]
    public void clientModifyHallucination(CSteamID senderId, short delta)
    {
        ReceiveModifyHallucination(delta);
    }

    /// <summary>
    /// Called from the server to induce a hallucination.
    /// </summary>
    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "clientModifyHallucination")]
    public void ReceiveModifyHallucination(short delta)
    {
        if (delta > 0)
        {
            askView((byte)delta);
        }
        else if (delta < 0)
        {
            askBlind((byte)(-delta));
        }
    }

    /// <summary>
    /// Add to or subtract from hallucination level on the client.
    /// </summary>
    public void serverModifyHallucination(float delta)
    {
        short num = MathfEx.RoundAndClampToShort(delta);
        if (num != 0)
        {
            SendModifyHallucination.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), num);
        }
    }

    [Obsolete("Use serverModifyHallucination instead.")]
    public void tellHallucinate(CSteamID senderId, byte amount)
    {
        clientModifyHallucination(senderId, amount);
    }

    [Obsolete("Use serverModifyHallucination instead.")]
    public void sendHallucination(byte amount)
    {
        serverModifyHallucination((int)amount);
    }

    /// <summary>
    /// Add to or subtract from warmth level.
    /// Does not replicate the change.
    /// </summary>
    public void simulatedModifyWarmth(short delta)
    {
        if (delta != 0 && !isDead)
        {
            if (delta > 0)
            {
                _warmth = (uint)(_warmth + delta);
            }
            else if (delta < 0)
            {
                _warmth = (uint)Mathf.Max(0, (int)_warmth + (int)delta);
            }
        }
    }

    [Obsolete]
    public void clientModifyWarmth(CSteamID senderId, short delta)
    {
        ReceiveModifyWarmth(delta);
    }

    /// <summary>
    /// Called from the server to modify warmth.
    /// </summary>
    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "clientModifyWarmth")]
    public void ReceiveModifyWarmth(short delta)
    {
        simulatedModifyWarmth(delta);
    }

    /// <summary>
    /// Add to or subtract from warmth level on the client and server.
    /// </summary>
    public void serverModifyWarmth(float delta)
    {
        short num = MathfEx.RoundAndClampToShort(delta);
        if (num != 0)
        {
            simulatedModifyWarmth(num);
            if (!base.channel.IsLocalPlayer)
            {
                SendModifyWarmth.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), num);
            }
        }
    }

    public void askBlind(byte amount)
    {
        if (amount != 0 && !isDead && IsAlive)
        {
            if (amount >= vision)
            {
                _vision = 0;
            }
            else
            {
                _vision -= amount;
            }
            if (vision == 0)
            {
                onVisionUpdated?.Invoke(isViewing: false);
            }
        }
    }

    public void askSuffocate(byte amount)
    {
        if (amount != 0 && !isDead && IsAlive)
        {
            lastSuffocate = base.player.input.simulation;
            if (amount >= oxygen)
            {
                _oxygen = 0;
            }
            else
            {
                _oxygen -= amount;
            }
            onOxygenUpdated?.Invoke(oxygen);
        }
    }

    public void askBreath(byte amount)
    {
        if (amount != 0 && !isDead && IsAlive)
        {
            if (amount >= 100 - oxygen)
            {
                _oxygen = 100;
            }
            else
            {
                _oxygen += amount;
            }
            onOxygenUpdated?.Invoke(oxygen);
        }
    }

    /// <summary>
    /// Add to or subtract from oxygen level.
    /// Does not replicate the change.
    /// </summary>
    public void simulatedModifyOxygen(sbyte delta)
    {
        if (delta > 0)
        {
            byte amount = (byte)delta;
            askBreath(amount);
        }
        else if (delta < 0)
        {
            byte amount2 = (byte)(-delta);
            askSuffocate(amount2);
        }
    }

    public void simulatedModifyOxygen(float delta)
    {
        simulatedModifyOxygen(MathfEx.RoundAndClampToSByte(delta));
    }

    /// <summary>
    /// Add to or subtract from health level.
    /// Replicates change to owner.
    /// </summary>
    public void serverModifyHealth(float delta)
    {
        if (delta > 0f)
        {
            byte amount = MathfEx.RoundAndClampToByte(delta);
            askHeal(amount, healBleeding: false, healBroken: false);
        }
        else
        {
            byte amount2 = MathfEx.RoundAndClampToByte(0f - delta);
            askDamage(amount2, Vector3.up, EDeathCause.SUICIDE, ELimb.SPINE, CSteamID.Nil, out var _, trackKill: false, ERagdollEffect.NONE, canCauseBleeding: false);
        }
    }

    /// <summary>
    /// Add to or subtract from food level.
    /// Replicates change to owner.
    /// </summary>
    public void serverModifyFood(float delta)
    {
        if (delta > 0f)
        {
            byte amount = MathfEx.RoundAndClampToByte(delta);
            askEat(amount);
        }
        else
        {
            byte amount2 = MathfEx.RoundAndClampToByte(0f - delta);
            askStarve(amount2);
        }
    }

    /// <summary>
    /// Add to or subtract from water level.
    /// Replicates change to owner.
    /// </summary>
    public void serverModifyWater(float delta)
    {
        if (delta > 0f)
        {
            byte amount = MathfEx.RoundAndClampToByte(delta);
            askDrink(amount);
        }
        else
        {
            byte amount2 = MathfEx.RoundAndClampToByte(0f - delta);
            askDehydrate(amount2);
        }
    }

    /// <summary>
    /// Add to or subtract from virus level.
    /// Replicates change to owner.
    /// </summary>
    public void serverModifyVirus(float delta)
    {
        if (delta > 0f)
        {
            byte amount = MathfEx.RoundAndClampToByte(delta);
            askDisinfect(amount);
        }
        else
        {
            byte amount2 = MathfEx.RoundAndClampToByte(0f - delta);
            askInfect(amount2);
        }
    }

    [Obsolete]
    public void askRespawn(CSteamID steamID, bool atHome)
    {
        ReceiveRespawnRequest(atHome);
    }

    /// <summary>
    /// Used by plugins to respawn the player bypassing timers. Issue #2701
    /// </summary>
    public void ServerRespawn(bool atHome)
    {
        if (IsAlive)
        {
            return;
        }
        sendRevive();
        if (!atHome || !BarricadeManager.tryGetBed(base.channel.owner.playerID.steamID, out var position, out var angle))
        {
            if (this.spawnpoint == null)
            {
                this.spawnpoint = LevelPlayers.getSpawn(isAlt: false);
            }
            if (this.spawnpoint == null)
            {
                position = base.transform.position;
                angle = 0;
            }
            else
            {
                position = this.spawnpoint.point;
                angle = MeasurementTool.angleToByte(this.spawnpoint.angle);
            }
            string npcSpawnId = base.player.quests.npcSpawnId;
            if (!string.IsNullOrEmpty(npcSpawnId))
            {
                Spawnpoint spawnpoint = SpawnpointSystemV2.Get().FindSpawnpoint(npcSpawnId);
                if (spawnpoint != null)
                {
                    position = spawnpoint.transform.position;
                    angle = MeasurementTool.angleToByte(spawnpoint.transform.rotation.eulerAngles.y);
                }
                else
                {
                    LocationDevkitNode locationDevkitNode = LocationDevkitNodeSystem.Get().FindByName(npcSpawnId);
                    if (locationDevkitNode != null)
                    {
                        position = locationDevkitNode.transform.position;
                        angle = MeasurementTool.angleToByte(UnityEngine.Random.Range(0f, 360f));
                    }
                    else
                    {
                        base.player.quests.npcSpawnId = null;
                        UnturnedLog.warn("Unable to find spawnpoint or location matching NpcSpawnId \"" + npcSpawnId + "\"");
                    }
                }
            }
        }
        if (PlayerLife.OnSelectingRespawnPoint != null)
        {
            float yaw = MeasurementTool.byteToAngle(angle);
            PlayerLife.OnSelectingRespawnPoint(this, atHome, ref position, ref yaw);
            angle = MeasurementTool.angleToByte(yaw);
        }
        position += new Vector3(0f, 0.5f, 0f);
        SendRevive.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), position, angle);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askRespawn")]
    public void ReceiveRespawnRequest(bool atHome)
    {
        if (!Provider.isServer || IsAlive)
        {
            return;
        }
        if (atHome)
        {
            if (Dedicator.IsDedicatedServer && Provider.isPvP)
            {
                if (Time.realtimeSinceStartup - lastDeath < (float)Provider.modeConfigData.Gameplay.Timer_Home)
                {
                    return;
                }
            }
            else if (Time.realtimeSinceStartup - lastRespawn < (float)Provider.modeConfigData.Gameplay.Timer_Respawn)
            {
                return;
            }
        }
        else if (Time.realtimeSinceStartup - lastRespawn < (float)Provider.modeConfigData.Gameplay.Timer_Respawn)
        {
            return;
        }
        ServerRespawn(atHome);
    }

    [Obsolete]
    public void askSuicide(CSteamID steamID)
    {
        ReceiveSuicideRequest();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askSuicide")]
    public void ReceiveSuicideRequest()
    {
        if (IsAlive && ((Level.info != null && Level.info.type == ELevelType.SURVIVAL) || !base.player.movement.isSafe || !base.player.movement.isSafeInfo.noWeapons) && Provider.modeConfigData.Gameplay.Can_Suicide)
        {
            doDamage(100, Vector3.up * 10f, EDeathCause.SUICIDE, ELimb.SKULL, base.channel.owner.playerID.steamID, out var _);
        }
    }

    /// <summary>
    /// Used to refill all client stats like stamina
    /// </summary>
    public void sendRevive()
    {
        _health = (byte)Provider.modeConfigData.Players.Health_Default;
        _food = (byte)Provider.modeConfigData.Players.Food_Default;
        _water = (byte)Provider.modeConfigData.Players.Water_Default;
        _virus = (byte)Provider.modeConfigData.Players.Virus_Default;
        _stamina = 100;
        _oxygen = 100;
        _vision = 0;
        _warmth = 0u;
        _isBleeding = false;
        _isBroken = false;
        _temperature = EPlayerTemperature.NONE;
        wasWarm = false;
        wasCovered = false;
        lastStarve = base.player.input.simulation;
        lastDehydrate = base.player.input.simulation;
        lastUncleaned = base.player.input.simulation;
        lastTire = base.player.input.simulation;
        lastRest = base.player.input.simulation;
        lastRadiate = base.player.input.simulation;
        lastOutsideDeadzoneFrame = base.player.input.simulation;
        pendingDeadzoneDamage = 0f;
        pendingDeadzoneRadiation = 0f;
        pendingDeadzoneMaskFilterQualityLoss = 0f;
        recentKiller = CSteamID.Nil;
        lastTimeAggressive = -100f;
        lastTimeTookDamage = -100f;
        lastTimeCausedDamage = -100f;
        SendLifeStats.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), health, food, water, virus, oxygen, isBleeding, isBroken);
    }

    public void sendRespawn(bool atHome)
    {
        SendRespawnRequest.Invoke(GetNetId(), ENetReliability.Unreliable, atHome);
    }

    public void sendSuicide()
    {
        SendSuicideRequest.Invoke(GetNetId(), ENetReliability.Unreliable);
    }

    internal void SimulateStaminaFrame(uint simulation)
    {
        if ((base.player.stance.stance == EPlayerStance.SPRINT || (base.player.stance.stance == EPlayerStance.DRIVING && base.player.movement.getVehicle() != null && base.player.movement.getVehicle().isBoosting)) && simulation - lastTire > 1 + base.player.skills.skills[0][4].level)
        {
            lastTire = simulation;
            askTire(1);
        }
        if (stamina < 100 && (float)(simulation - lastTire) > 32f * (1f - base.player.skills.mastery(0, 3) * 0.5f) && simulation - lastRest > 1)
        {
            lastRest = simulation;
            askRest((byte)(1f + base.player.skills.mastery(0, 3) * 2f));
        }
    }

    private void SetIsAsphyxiating(bool newIsAsphyxiating)
    {
        if (isAsphyxiating != newIsAsphyxiating)
        {
            isAsphyxiating = newIsAsphyxiating;
            this.OnIsAsphyxiatingChanged?.Invoke();
        }
    }

    private void SimulateOxygenFrame(uint simulation)
    {
        Vector3 position = base.transform.position;
        float num;
        if (OxygenManager.checkPointBreathable(position))
        {
            num = 1f;
        }
        else
        {
            if (base.player.stance.isSubmerged)
            {
                num = -1f;
            }
            else if (Level.info != null && Level.info.type == ELevelType.SURVIVAL)
            {
                if (Level.info.configData != null && Level.info.configData.Use_Legacy_Oxygen_Height)
                {
                    float waterSurfaceElevation = LevelLighting.getWaterSurfaceElevation(0f);
                    float t = Mathf.Clamp01((position.y - waterSurfaceElevation) / (Level.HEIGHT - waterSurfaceElevation));
                    num = Mathf.Lerp(1f, -1f, t);
                }
                else
                {
                    num = 1f;
                }
            }
            else
            {
                num = 1f;
            }
            if (num > -0.9999f && VolumeManager<OxygenVolume, OxygenVolumeManager>.Get().IsPositionInsideNonBreathableVolume(position, out var maxAlpha))
            {
                num = Mathf.Lerp(num, -1f, maxAlpha);
            }
            if (num < 0.9999f && VolumeManager<OxygenVolume, OxygenVolumeManager>.Get().IsPositionInsideBreathableVolume(position, out var maxAlpha2))
            {
                num = Mathf.Lerp(num, 1f, maxAlpha2);
            }
        }
        if (num > 0f)
        {
            SetIsAsphyxiating(newIsAsphyxiating: false);
            if (oxygen < 100 && simulation - lastBreath > (uint)(1 + Mathf.CeilToInt(10f * (1f - num))))
            {
                lastBreath = simulation;
                askBreath((byte)(1f + base.player.skills.mastery(0, 3) * 2f));
            }
        }
        else
        {
            if (!(num < 0f))
            {
                return;
            }
            SetIsAsphyxiating(newIsAsphyxiating: true);
            if (oxygen > 0)
            {
                uint num2 = (uint)(1 + base.player.skills.skills[0][5].level);
                num2 += (uint)Mathf.CeilToInt((num + 1f) * 10f);
                if (base.player.clothing.backpackAsset != null && base.player.clothing.backpackAsset.proofWater && ((base.player.clothing.glassesAsset != null && base.player.clothing.glassesAsset.proofWater) || (base.player.clothing.maskAsset != null && base.player.clothing.maskAsset.proofWater)))
                {
                    num2 *= 10;
                }
                if (simulation - lastSuffocate > num2)
                {
                    lastSuffocate = simulation;
                    askSuffocate(1);
                }
            }
            else if (simulation - lastSuffocate > 10)
            {
                lastSuffocate = simulation;
                if (Provider.isServer)
                {
                    doDamage(10, Vector3.up, EDeathCause.BREATH, ELimb.SPINE, Provider.server, out var _);
                }
            }
        }
    }

    public void simulate(uint simulation)
    {
        if (Provider.isServer)
        {
            if (Level.info.type == ELevelType.SURVIVAL)
            {
                if (food > 0)
                {
                    if ((float)(simulation - lastStarve) > (float)Provider.modeConfigData.Players.Food_Use_Ticks * (1f + base.player.skills.mastery(1, 6) * 0.25f) * (base.player.movement.inSnow ? (0.5f + base.player.skills.mastery(1, 5) * 0.5f) : 1f))
                    {
                        lastStarve = simulation;
                        askStarve(1);
                    }
                }
                else if (simulation - lastStarve > Provider.modeConfigData.Players.Food_Damage_Ticks)
                {
                    lastStarve = simulation;
                    askDamage(1, Vector3.up, EDeathCause.FOOD, ELimb.SPINE, Provider.server, out var _);
                }
                if (water > 0)
                {
                    if ((float)(simulation - lastDehydrate) > (float)Provider.modeConfigData.Players.Water_Use_Ticks * (1f + base.player.skills.mastery(1, 6) * 0.25f))
                    {
                        lastDehydrate = simulation;
                        askDehydrate(1);
                    }
                }
                else if (simulation - lastDehydrate > Provider.modeConfigData.Players.Water_Damage_Ticks)
                {
                    lastDehydrate = simulation;
                    askDamage(1, Vector3.up, EDeathCause.WATER, ELimb.SPINE, Provider.server, out var _);
                }
                if (virus == 0)
                {
                    if (simulation - lastInfect > Provider.modeConfigData.Players.Virus_Damage_Ticks)
                    {
                        lastInfect = simulation;
                        askDamage(1, Vector3.up, EDeathCause.INFECTION, ELimb.SPINE, Provider.server, out var _);
                    }
                }
                else if (virus < Provider.modeConfigData.Players.Virus_Infect && simulation - lastUncleaned > Provider.modeConfigData.Players.Virus_Use_Ticks)
                {
                    lastUncleaned = simulation;
                    askInfect(1);
                }
            }
            if (isBleeding)
            {
                if (simulation - lastBleed > Provider.modeConfigData.Players.Bleed_Damage_Ticks)
                {
                    lastBleed = simulation;
                    askDamage(1, Vector3.up, EDeathCause.BLEEDING, ELimb.SPINE, Provider.server, out var _);
                }
            }
            else if (health < 100 && food > Provider.modeConfigData.Players.Health_Regen_Min_Food && water > Provider.modeConfigData.Players.Health_Regen_Min_Water && (float)(simulation - lastRegenerate) > (float)Provider.modeConfigData.Players.Health_Regen_Ticks * (1f - base.player.skills.mastery(1, 1) * 0.5f))
            {
                lastRegenerate = simulation;
                askHeal(1, healBleeding: false, healBroken: false);
            }
            if (Provider.modeConfigData.Players.Can_Stop_Bleeding && isBleeding && (float)(simulation - lastBleeding) > (float)Provider.modeConfigData.Players.Bleed_Regen_Ticks * (1f - base.player.skills.mastery(1, 4) * 0.5f))
            {
                serverSetBleeding(newBleeding: false);
            }
            if (Provider.modeConfigData.Players.Can_Fix_Legs && isBroken && (float)(simulation - lastBroken) > (float)Provider.modeConfigData.Players.Leg_Regen_Ticks * (1f - base.player.skills.mastery(1, 4) * 0.5f))
            {
                serverSetLegsBroken(newLegsBroken: false);
            }
        }
        if (base.channel.IsLocalPlayer)
        {
            if (vision > 0 && simulation - lastView > 12)
            {
                lastView = simulation;
                askBlind(1);
            }
            if (IsAlive)
            {
                Provider.provider.economyService.updateInventory();
            }
        }
        if (!base.channel.IsLocalPlayer && !Provider.isServer)
        {
            return;
        }
        SimulateStaminaFrame(simulation);
        SimulateOxygenFrame(simulation);
        if (base.player.movement.isRadiated)
        {
            bool flag = base.player.clothing.maskAsset != null && base.player.clothing.maskAsset.proofRadiation && base.player.clothing.maskQuality > 0;
            if (base.player.movement.ActiveDeadzone.DeadzoneType == EDeadzoneType.FullSuitRadiation)
            {
                flag &= base.player.clothing.shirtAsset != null && base.player.clothing.shirtAsset.proofRadiation;
                flag &= base.player.clothing.pantsAsset != null && base.player.clothing.pantsAsset.proofRadiation;
            }
            if (flag)
            {
                if (simulation - lastOutsideDeadzoneFrame > 2)
                {
                    pendingDeadzoneDamage += base.player.movement.ActiveDeadzone.ProtectedDamagePerSecond * PlayerInput.RATE;
                    float maskFilterDamagePerSecond = base.player.movement.ActiveDeadzone.MaskFilterDamagePerSecond;
                    maskFilterDamagePerSecond *= base.player.clothing.maskAsset.FilterDegradationRateMultiplier;
                    pendingDeadzoneMaskFilterQualityLoss += maskFilterDamagePerSecond * PlayerInput.RATE;
                    int num = Mathf.FloorToInt(pendingDeadzoneMaskFilterQualityLoss);
                    if (num > 0)
                    {
                        pendingDeadzoneMaskFilterQualityLoss -= num;
                        lastRadiate = simulation;
                        base.player.clothing.maskQuality--;
                        base.player.clothing.updateMaskQuality();
                    }
                }
            }
            else
            {
                if (simulation - lastOutsideDeadzoneFrame > 2)
                {
                    pendingDeadzoneDamage += base.player.movement.ActiveDeadzone.UnprotectedDamagePerSecond * PlayerInput.RATE;
                }
                if (virus > 0)
                {
                    if (simulation - lastRadiate > 1)
                    {
                        lastRadiate = simulation;
                    }
                    if (simulation - lastOutsideDeadzoneFrame > 2)
                    {
                        pendingDeadzoneRadiation += base.player.movement.ActiveDeadzone.UnprotectedRadiationPerSecond * PlayerInput.RATE;
                    }
                    int num2 = Mathf.FloorToInt(pendingDeadzoneRadiation);
                    if (num2 > 0)
                    {
                        pendingDeadzoneRadiation -= num2;
                        askRadiate(MathfEx.ClampToByte(num2));
                    }
                }
                else if (Provider.isServer && simulation - lastRadiate > 10)
                {
                    lastRadiate = simulation;
                    askDamage(10, Vector3.up, EDeathCause.INFECTION, ELimb.SPINE, Provider.server, out var _);
                }
            }
            if (!isDead && Provider.isServer)
            {
                int num3 = Mathf.FloorToInt(pendingDeadzoneDamage);
                if (num3 > 0)
                {
                    pendingDeadzoneDamage -= num3;
                    askDamage(MathfEx.ClampToByte(num3), Vector3.up, EDeathCause.INFECTION, ELimb.SPINE, Provider.server, out var _);
                }
            }
        }
        else
        {
            lastRadiate = simulation;
            lastOutsideDeadzoneFrame = simulation;
            pendingDeadzoneDamage = 0f;
            pendingDeadzoneRadiation = 0f;
            pendingDeadzoneMaskFilterQualityLoss = 0f;
        }
        if (warmth != 0)
        {
            simulatedModifyWarmth(-1);
        }
        bool proofFire = false;
        if (base.player.clothing.shirtAsset != null && base.player.clothing.shirtAsset.proofFire && base.player.clothing.pantsAsset != null && base.player.clothing.pantsAsset.proofFire)
        {
            proofFire = true;
        }
        _ = temperature;
        EPlayerTemperature ePlayerTemperature = TemperatureManager.checkPointTemperature(base.transform.position, proofFire);
        EPlayerTemperature ePlayerTemperature2;
        if (ePlayerTemperature == EPlayerTemperature.ACID)
        {
            ePlayerTemperature2 = EPlayerTemperature.ACID;
            if (Provider.isServer && simulation - lastBurn > 10)
            {
                lastBurn = simulation;
                askDamage(10, Vector3.up, EDeathCause.SPIT, ELimb.SPINE, Provider.server, out var _);
            }
        }
        else if (ePlayerTemperature == EPlayerTemperature.BURNING)
        {
            ePlayerTemperature2 = EPlayerTemperature.BURNING;
            if (Provider.isServer && simulation - lastBurn > 10)
            {
                lastBurn = simulation;
                askDamage(10, Vector3.up, EDeathCause.BURNING, ELimb.SPINE, Provider.server, out var _);
            }
            lastWarm = simulation;
            wasWarm = true;
        }
        else if (ePlayerTemperature == EPlayerTemperature.WARM || warmth != 0)
        {
            ePlayerTemperature2 = EPlayerTemperature.WARM;
            lastWarm = simulation;
            wasWarm = true;
        }
        else if (base.player.movement.inSnow && Level.info != null && Level.info.configData.Snow_Affects_Temperature)
        {
            if (base.player.stance.stance == EPlayerStance.SWIM)
            {
                ePlayerTemperature2 = EPlayerTemperature.FREEZING;
                if (Provider.isServer && simulation - lastFreeze > 25)
                {
                    lastFreeze = simulation;
                    byte b = 8;
                    if (base.player.clothing.shirtAsset != null || base.player.clothing.vestAsset != null)
                    {
                        b -= 2;
                    }
                    if (base.player.clothing.pantsAsset != null)
                    {
                        b -= 2;
                    }
                    if (base.player.clothing.hatAsset != null)
                    {
                        b -= 2;
                    }
                    askDamage(b, Vector3.up, EDeathCause.FREEZING, ELimb.SPINE, Provider.server, out var _);
                }
            }
            else if (!wasWarm || (float)(simulation - lastWarm) > 250f * (1f + base.player.skills.mastery(1, 5)))
            {
                if ((base.player.movement.getVehicle() != null && !base.player.movement.getVehicle().asset.hasZip && !base.player.movement.getVehicle().asset.hasBicycle) || Physics.Raycast(base.transform.position + Vector3.up, Quaternion.Euler(45f, LevelLighting.wind, 0f) * -Vector3.forward, 32f, RayMasks.BLOCK_WIND))
                {
                    ePlayerTemperature2 = EPlayerTemperature.COVERED;
                    lastCovered = simulation;
                    wasCovered = true;
                }
                else
                {
                    byte b2 = 1;
                    if (base.player.clothing.shirtAsset != null || base.player.clothing.vestAsset != null)
                    {
                        b2++;
                    }
                    if (base.player.clothing.pantsAsset != null)
                    {
                        b2++;
                    }
                    if (base.player.clothing.hatAsset != null)
                    {
                        b2++;
                    }
                    if (!wasCovered || simulation - lastCovered > 50 * b2)
                    {
                        ePlayerTemperature2 = EPlayerTemperature.FREEZING;
                        if (Provider.isServer && simulation - lastFreeze > 75)
                        {
                            lastFreeze = simulation;
                            byte b3 = 4;
                            if (base.player.clothing.shirtAsset != null || base.player.clothing.vestAsset != null)
                            {
                                b3--;
                            }
                            if (base.player.clothing.pantsAsset != null)
                            {
                                b3--;
                            }
                            if (base.player.clothing.hatAsset != null)
                            {
                                b3--;
                            }
                            askDamage(b3, Vector3.up, EDeathCause.FREEZING, ELimb.SPINE, Provider.server, out var _);
                        }
                    }
                    else
                    {
                        ePlayerTemperature2 = EPlayerTemperature.COLD;
                    }
                }
            }
            else
            {
                ePlayerTemperature2 = EPlayerTemperature.COLD;
                lastCovered = simulation;
                wasCovered = true;
            }
        }
        else
        {
            ePlayerTemperature2 = EPlayerTemperature.NONE;
        }
        if (ePlayerTemperature2 != temperature)
        {
            _temperature = ePlayerTemperature2;
            onTemperatureUpdated?.Invoke(temperature);
        }
    }

    public void breakLegs()
    {
        if (!isBroken)
        {
            EffectAsset effectAsset = BonesRef.Find();
            if (effectAsset != null)
            {
                TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
                parameters.relevantDistance = EffectManager.SMALL;
                parameters.position = base.transform.position;
                EffectManager.triggerEffect(parameters);
            }
        }
        serverSetLegsBroken(newLegsBroken: true);
    }

    private void onLanded(float velocity)
    {
        LevelAsset asset = Level.getAsset();
        float num = ((asset != null && asset.fallDamageSpeedThreshold > 0.01f) ? asset.fallDamageSpeedThreshold : 22f);
        if (!(velocity < 0f - num) || !(base.player.movement.totalGravityMultiplier > 0.67f))
        {
            return;
        }
        Transform transform = base.player.movement.ground.transform;
        ObjectAsset objectAsset = ((transform != null) ? LevelObjects.getAsset(transform) : null);
        if (objectAsset != null && !objectAsset.causesFallDamage)
        {
            return;
        }
        if (transform != null)
        {
            FallDamageOverride componentInParent = transform.gameObject.GetComponentInParent<FallDamageOverride>();
            if (componentInParent != null)
            {
                switch (componentInParent.Mode)
                {
                case FallDamageOverride.EMode.PreventFallDamage:
                    return;
                default:
                    UnturnedLog.warn("Unknown fall damage override: {0}", componentInParent.GetSceneHierarchyPath());
                    break;
                case FallDamageOverride.EMode.None:
                    break;
                }
            }
        }
        float num2 = Mathf.Min(101f, Mathf.Abs(velocity));
        float num3 = 1f - base.player.skills.mastery(1, 4) * 0.75f;
        float num4 = num2 * num3;
        num4 *= base.player.clothing.fallingDamageMultiplier;
        if (!Provider.modeConfigData.Players.Can_Hurt_Legs)
        {
            num4 = 0f;
        }
        bool can_Break_Legs = Provider.modeConfigData.Players.Can_Break_Legs;
        can_Break_Legs &= !base.player.clothing.preventsFallingBrokenBones;
        if (this.OnFallDamageRequested != null)
        {
            try
            {
                this.OnFallDamageRequested(this, velocity, ref num4, ref can_Break_Legs);
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, "Caught exception during OnFallDamageRequested:");
            }
        }
        byte b = MathfEx.RoundAndClampToByte(num4);
        if (b > 0)
        {
            askDamage(b, Vector3.down, EDeathCause.BONES, ELimb.SPINE, Provider.server, out var _);
        }
        if (can_Break_Legs)
        {
            breakLegs();
        }
    }

    internal void InitializePlayer()
    {
        if (Provider.isServer)
        {
            PlayerMovement movement = base.player.movement;
            movement.onLanded = (Landed)Delegate.Combine(movement.onLanded, new Landed(onLanded));
            load();
        }
    }

    public void load()
    {
        wasLoadCalled = true;
        _isDead = false;
        if (PlayerSavedata.fileExists(base.channel.owner.playerID, "/Player/Life.dat") && Level.info.type == ELevelType.SURVIVAL)
        {
            Block block = PlayerSavedata.readBlock(base.channel.owner.playerID, "/Player/Life.dat", 0);
            byte b = block.readByte();
            if (b > 1)
            {
                _health = block.readByte();
                _food = block.readByte();
                _water = block.readByte();
                _virus = block.readByte();
                _stamina = 100;
                if (b < SAVEDATA_VERSION_WITH_OXYGEN)
                {
                    _oxygen = 100;
                }
                else
                {
                    _oxygen = block.readByte();
                }
                _isBleeding = block.readBoolean();
                _isBroken = block.readBoolean();
                _temperature = EPlayerTemperature.NONE;
                wasWarm = false;
                wasCovered = false;
                return;
            }
        }
        _health = (byte)Provider.modeConfigData.Players.Health_Default;
        _food = (byte)Provider.modeConfigData.Players.Food_Default;
        _water = (byte)Provider.modeConfigData.Players.Water_Default;
        _virus = (byte)Provider.modeConfigData.Players.Virus_Default;
        _stamina = 100;
        _oxygen = 100;
        _isBleeding = false;
        _isBroken = false;
        _temperature = EPlayerTemperature.NONE;
        wasWarm = false;
        wasCovered = false;
        recentKiller = CSteamID.Nil;
        lastTimeAggressive = -100f;
        lastTimeTookDamage = -100f;
        lastTimeCausedDamage = -100f;
    }

    public void save()
    {
        if (!wasLoadCalled)
        {
            return;
        }
        if (base.player.life.isDead)
        {
            if (PlayerSavedata.fileExists(base.channel.owner.playerID, "/Player/Life.dat"))
            {
                PlayerSavedata.deleteFile(base.channel.owner.playerID, "/Player/Life.dat");
            }
            return;
        }
        Block block = new Block();
        block.writeByte(SAVEDATA_VERSION_LATEST);
        block.writeByte(health);
        block.writeByte(food);
        block.writeByte(water);
        block.writeByte(virus);
        block.writeByte(oxygen);
        block.writeBoolean(isBleeding);
        block.writeBoolean(isBroken);
        PlayerSavedata.writeBlock(base.channel.owner.playerID, "/Player/Life.dat", block);
    }
}
