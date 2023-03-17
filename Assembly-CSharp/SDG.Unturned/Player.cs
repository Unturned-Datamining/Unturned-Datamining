using System;
using System.Collections;
using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class Player : MonoBehaviour
{
    public delegate void PlayerStatIncremented(Player player, EPlayerStat stat);

    public delegate void PluginWidgetFlagsChanged(Player player, EPluginWidgetFlags oldFlags);

    public static readonly byte SAVEDATA_VERSION = 1;

    public static PlayerCreated onPlayerCreated;

    public PlayerTeleported onPlayerTeleported;

    public PlayerSpyReady onPlayerSpyReady;

    public static PlayerSpyReady onSpyReady;

    public static bool isLoadingInventory;

    public static bool isLoadingLife;

    public static bool isLoadingClothing;

    public int agro;

    private static Player _player;

    protected SteamChannel _channel;

    private PlayerAnimator _animator;

    private PlayerClothing _clothing;

    private PlayerInventory _inventory;

    private PlayerEquipment _equipment;

    private PlayerLife _life;

    private PlayerCrafting _crafting;

    private PlayerSkills _skills;

    private PlayerMovement _movement;

    private PlayerLook _look;

    private PlayerStance _stance;

    private PlayerInput _input;

    private PlayerVoice _voice;

    private PlayerInteract _interact;

    private PlayerWorkzone _workzone;

    private PlayerQuests _quests;

    private Transform _first;

    private Transform _third;

    private Transform _character;

    private Transform firstSpot;

    private Transform thirdSpot;

    private bool itemOn;

    private PlayerSpotLightConfig itemLightConfig;

    private bool headlampOn;

    private PlayerSpotLightConfig headlampLightConfig;

    private int screenshotsExpected;

    private CSteamID screenshotsDestination;

    private Queue<PlayerSpyReady> screenshotsCallbacks = new Queue<PlayerSpyReady>();

    private static readonly ClientInstanceMethod SendScreenshotDestination = ClientInstanceMethod.Get(typeof(Player), "ReceiveScreenshotDestination");

    private static readonly ServerInstanceMethod SendScreenshotRelay = ServerInstanceMethod.Get(typeof(Player), "ReceiveScreenshotRelay");

    private Texture2D screenshotFinal;

    private static readonly ClientInstanceMethod SendTakeScreenshot = ClientInstanceMethod.Get(typeof(Player), "ReceiveTakeScreenshot");

    private static readonly ClientInstanceMethod<string, string> SendBrowserRequest = ClientInstanceMethod<string, string>.Get(typeof(Player), "ReceiveBrowserRequest");

    private static readonly ClientInstanceMethod<string, float> SendHintMessage = ClientInstanceMethod<string, float>.Get(typeof(Player), "ReceiveHintMessage");

    private static readonly ClientInstanceMethod<uint, ushort, string, bool> SendRelayToServer = ClientInstanceMethod<uint, ushort, string, bool>.Get(typeof(Player), "ReceiveRelayToServer");

    private static readonly ClientInstanceMethod<uint> SendSetPluginWidgetFlags = ClientInstanceMethod<uint>.Get(typeof(Player), "ReceiveSetPluginWidgetFlags");

    private static readonly ServerInstanceMethod SendBattlEyeLogsRequest = ServerInstanceMethod.Get(typeof(Player), "ReceiveBattlEyeLogsRequest");

    private static readonly ClientInstanceMethod<string> SendTerminalRelay = ClientInstanceMethod<string>.Get(typeof(Player), "ReceiveTerminalRelay");

    internal const float TELEPORT_VERTICAL_OFFSET = 0.5f;

    private static readonly ClientInstanceMethod<Vector3, byte> SendTeleport = ClientInstanceMethod<Vector3, byte>.Get(typeof(Player), "ReceiveTeleport");

    private static readonly ClientInstanceMethod<EPlayerStat> SendStat = ClientInstanceMethod<EPlayerStat>.Get(typeof(Player), "ReceiveStat");

    private static readonly ClientInstanceMethod<string> SendAchievementUnlocked = ClientInstanceMethod<string>.Get(typeof(Player), "ReceiveAchievementUnlocked");

    private static readonly ClientInstanceMethod<EPlayerMessage> SendUIMessage = ClientInstanceMethod<EPlayerMessage>.Get(typeof(Player), "ReceiveUIMessage");

    public uint maxRateLimitedActionsPerSecond = 10u;

    private NetId _netId;

    public static bool isLoading
    {
        get
        {
            if (!isLoadingLife && !isLoadingInventory)
            {
                return isLoadingClothing;
            }
            return true;
        }
    }

    public static Player player => _player;

    public static Player instance => player;

    public SteamChannel channel => _channel;

    public PlayerAnimator animator => _animator;

    public PlayerClothing clothing => _clothing;

    public PlayerInventory inventory => _inventory;

    public PlayerEquipment equipment => _equipment;

    public PlayerLife life => _life;

    public PlayerCrafting crafting => _crafting;

    public PlayerSkills skills => _skills;

    public PlayerMovement movement => _movement;

    public PlayerLook look => _look;

    public PlayerStance stance => _stance;

    public PlayerInput input => _input;

    public PlayerVoice voice => _voice;

    public PlayerInteract interact => _interact;

    public PlayerWorkzone workzone => _workzone;

    public PlayerQuests quests => _quests;

    public Transform first => _first;

    public Transform third => _third;

    public Transform character => _character;

    public bool isSpotOn
    {
        get
        {
            if (!itemOn)
            {
                return headlampOn;
            }
            return true;
        }
    }

    private PlayerSpotLightConfig lightConfig
    {
        get
        {
            if (itemOn && headlampOn)
            {
                PlayerSpotLightConfig result = default(PlayerSpotLightConfig);
                result.angle = Mathf.LerpAngle(itemLightConfig.angle, headlampLightConfig.angle, 0.5f);
                result.color = Color.Lerp(itemLightConfig.color, headlampLightConfig.color, 0.5f);
                result.intensity = Mathf.Lerp(itemLightConfig.intensity, headlampLightConfig.intensity, 0.5f);
                result.range = Mathf.Lerp(itemLightConfig.range, headlampLightConfig.range, 0.5f);
                return result;
            }
            if (itemOn)
            {
                return itemLightConfig;
            }
            if (headlampOn)
            {
                return headlampLightConfig;
            }
            return default(PlayerSpotLightConfig);
        }
    }

    public bool inPluginModal => isPluginWidgetFlagActive(EPluginWidgetFlags.Modal);

    public EPluginWidgetFlags pluginWidgetFlags { get; protected set; } = EPluginWidgetFlags.Default;


    public bool wantsBattlEyeLogs { get; protected set; }

    public float rateLimitedActionsCredits { get; protected set; }

    public static event PlayerStatIncremented onPlayerStatIncremented;

    public event PluginWidgetFlagsChanged onLocalPluginWidgetFlagsChanged;

    public void PlayAudioReference(AudioReference audioReference)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            float volumeMultiplier;
            float pitchMultiplier;
            AudioClip audioClip = audioReference.LoadAudioClip(out volumeMultiplier, out pitchMultiplier);
            if (!(audioClip == null))
            {
                OneShotAudioParameters oneShotAudioParameters = new OneShotAudioParameters(base.transform, audioClip);
                oneShotAudioParameters.volume = volumeMultiplier;
                oneShotAudioParameters.pitch = pitchMultiplier;
                oneShotAudioParameters.SetLinearRolloff(1f, 32f);
                oneShotAudioParameters.Play();
            }
        }
    }

    public void playSound(AudioClip clip, float volume, float pitch, float deviation)
    {
        if (!(clip == null) && !Dedicator.IsDedicatedServer)
        {
            deviation = Mathf.Clamp01(deviation);
            OneShotAudioParameters oneShotAudioParameters = new OneShotAudioParameters(base.transform, clip);
            oneShotAudioParameters.volume = volume;
            oneShotAudioParameters.RandomizePitch(pitch * (1f - deviation), pitch * (1f + deviation));
            oneShotAudioParameters.SetLinearRolloff(1f, 32f);
            oneShotAudioParameters.Play();
        }
    }

    public void playSound(AudioClip clip, float pitch, float deviation)
    {
        playSound(clip, 1f, pitch, deviation);
    }

    public void playSound(AudioClip clip, float volume)
    {
        playSound(clip, volume, 1f, 0.1f);
    }

    public void playSound(AudioClip clip)
    {
        playSound(clip, 1f, 1f, 0.1f);
    }

    [Obsolete]
    public void tellScreenshotDestination(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveScreenshotDestination(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt16(out var value);
        byte[] array = new byte[value];
        reader.ReadBytes(array);
        HandleScreenshotData(array);
    }

    private void HandleScreenshotData(byte[] data)
    {
        if (Dedicator.IsDedicatedServer)
        {
            ReadWrite.writeBytes(ReadWrite.PATH + ServerSavedata.directory + "/" + Provider.serverID + "/Spy.jpg", useCloud: false, usePath: false, data);
            ReadWrite.writeBytes(ReadWrite.PATH + ServerSavedata.directory + "/" + Provider.serverID + "/Spy/" + channel.owner.playerID.steamID.m_SteamID + ".jpg", useCloud: false, usePath: false, data);
            onPlayerSpyReady?.Invoke(channel.owner.playerID.steamID, data);
            screenshotsCallbacks.Dequeue()?.Invoke(channel.owner.playerID.steamID, data);
        }
        else
        {
            ReadWrite.writeBytes("/Spy.jpg", useCloud: false, usePath: true, data);
            onSpyReady?.Invoke(channel.owner.playerID.steamID, data);
        }
    }

    [Obsolete]
    public void tellScreenshotRelay(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER)]
    public void ReceiveScreenshotRelay(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (screenshotsExpected < 1)
        {
            return;
        }
        screenshotsExpected--;
        if (!reader.ReadUInt16(out var length))
        {
            return;
        }
        byte[] data = new byte[length];
        reader.ReadBytes(data);
        if (screenshotsDestination != CSteamID.Nil)
        {
            ITransportConnection transportConnection = Provider.findTransportConnection(screenshotsDestination);
            if (transportConnection != null)
            {
                SendScreenshotDestination.Invoke(GetNetId(), ENetReliability.Reliable, transportConnection, delegate(NetPakWriter writer)
                {
                    writer.WriteUInt16(length);
                    writer.WriteBytes(data);
                });
            }
        }
        HandleScreenshotData(data);
    }

    private IEnumerator takeScreenshot()
    {
        yield return new WaitForEndOfFrame();
        Texture2D texture2D = ScreenCapture.CaptureScreenshotAsTexture();
        RenderTexture temporary = RenderTexture.GetTemporary(640, 480, 0, texture2D.graphicsFormat);
        Graphics.Blit(texture2D, temporary);
        UnityEngine.Object.Destroy(texture2D);
        if (screenshotFinal == null)
        {
            screenshotFinal = new Texture2D(640, 480, TextureFormat.RGB24, mipChain: false);
            screenshotFinal.name = "Screenshot_Final";
            screenshotFinal.hideFlags = HideFlags.HideAndDontSave;
        }
        RenderTexture.active = temporary;
        screenshotFinal.ReadPixels(new Rect(0f, 0f, screenshotFinal.width, screenshotFinal.height), 0, 0, recalculateMipMaps: false);
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(temporary);
        byte[] data = screenshotFinal.EncodeToJPG(33);
        if (data.Length >= 30000)
        {
            yield break;
        }
        if (Provider.isServer)
        {
            HandleScreenshotData(data);
            yield break;
        }
        SendScreenshotRelay.Invoke(GetNetId(), ENetReliability.Reliable, delegate(NetPakWriter writer)
        {
            ushort num = (ushort)data.Length;
            writer.WriteUInt16(num);
            writer.WriteBytes(data, num);
        });
    }

    [Obsolete]
    public void askScreenshot(CSteamID steamID)
    {
        ReceiveTakeScreenshot();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askScreenshot")]
    public void ReceiveTakeScreenshot()
    {
        StartCoroutine(takeScreenshot());
    }

    public void sendScreenshot(CSteamID destination, PlayerSpyReady callback = null)
    {
        screenshotsExpected++;
        screenshotsDestination = destination;
        screenshotsCallbacks.Enqueue(callback);
        SendTakeScreenshot.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection());
    }

    [Obsolete]
    public void askBrowserRequest(CSteamID steamID, string msg, string url)
    {
        ReceiveBrowserRequest(msg, url);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askBrowserRequest")]
    public void ReceiveBrowserRequest(string msg, string url)
    {
        if (!WebUtils.ParseThirdPartyUrl(url, out var result))
        {
            UnturnedLog.warn("Ignoring potentially unsafe browser request \"{0}\" \"{1}\"", msg, url);
        }
        else if (PlayerUI.instance != null)
        {
            PlayerUI.instance.browserRequestUI.open(msg, result);
            PlayerLifeUI.close();
        }
    }

    public void sendBrowserRequest(string msg, string url)
    {
        SendBrowserRequest.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), msg, url);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveHintMessage(string message, float durationSeconds)
    {
        if (PlayerUI.instance != null)
        {
            PlayerUI.message(EPlayerMessage.NPC_CUSTOM, message, durationSeconds);
        }
    }

    public void ServerShowHint(string message, float durationSeconds)
    {
        SendHintMessage.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), message, durationSeconds);
    }

    [Obsolete]
    public void askRelayToServer(CSteamID steamID, uint ip, ushort port, string password, bool shouldShowMenu)
    {
        ReceiveRelayToServer(ip, port, password, shouldShowMenu);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askRelayToServer")]
    public void ReceiveRelayToServer(uint ip, ushort port, string password, bool shouldShowMenu)
    {
        if (!MenuPlayConnectUI.hasPendingServerRelay)
        {
            if (Provider.isServer)
            {
                throw new NotSupportedException($"IP: {Parser.getIPFromUInt32(ip)} Port: {port}");
            }
            MenuPlayConnectUI.hasPendingServerRelay = true;
            MenuPlayConnectUI.serverRelayIP = ip;
            MenuPlayConnectUI.serverRelayPort = port;
            MenuPlayConnectUI.serverRelayPassword = password;
            MenuPlayConnectUI.serverRelayWaitOnMenu = shouldShowMenu;
            Provider.RequestDisconnect($"Relaying to IP: {Parser.getIPFromUInt32(ip)} Port: {port}");
        }
    }

    public void sendRelayToServer(uint ip, ushort port, string password, bool shouldShowMenu = true)
    {
        SendRelayToServer.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), ip, port, password, shouldShowMenu);
    }

    public void sendRelayToServer(uint ip, ushort port, string password)
    {
        sendRelayToServer(ip, port, password, shouldShowMenu: true);
    }

    public bool isPluginWidgetFlagActive(EPluginWidgetFlags flag)
    {
        return (pluginWidgetFlags & flag) == flag;
    }

    [Obsolete]
    public void clientsideSetPluginWidgetFlags(CSteamID steamID, uint newFlags)
    {
        ReceiveSetPluginWidgetFlags(newFlags);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "clientsideSetPluginWidgetFlags")]
    public void ReceiveSetPluginWidgetFlags(uint newFlags)
    {
        EPluginWidgetFlags oldFlags = pluginWidgetFlags;
        pluginWidgetFlags = (EPluginWidgetFlags)newFlags;
        this.onLocalPluginWidgetFlagsChanged?.Invoke(this, oldFlags);
    }

    public void setAllPluginWidgetFlags(EPluginWidgetFlags newFlags)
    {
        if (pluginWidgetFlags != newFlags)
        {
            SendSetPluginWidgetFlags.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), (uint)newFlags);
            pluginWidgetFlags = newFlags;
        }
    }

    public void enablePluginWidgetFlag(EPluginWidgetFlags flag)
    {
        EPluginWidgetFlags allPluginWidgetFlags = pluginWidgetFlags | flag;
        setAllPluginWidgetFlags(allPluginWidgetFlags);
    }

    public void disablePluginWidgetFlag(EPluginWidgetFlags flag)
    {
        EPluginWidgetFlags allPluginWidgetFlags = pluginWidgetFlags & ~flag;
        setAllPluginWidgetFlags(allPluginWidgetFlags);
    }

    public void setPluginWidgetFlag(EPluginWidgetFlags flag, bool active)
    {
        if (active)
        {
            enablePluginWidgetFlag(flag);
        }
        else
        {
            disablePluginWidgetFlag(flag);
        }
    }

    [Obsolete]
    public void serversideSetPluginModal(bool enableModal)
    {
        setPluginWidgetFlag(EPluginWidgetFlags.Modal, enableModal);
    }

    [Obsolete]
    public void askRequestBattlEyeLogs(CSteamID steamID)
    {
        ReceiveBattlEyeLogsRequest();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 1, legacyName = "askRequestBattlEyeLogs")]
    public void ReceiveBattlEyeLogsRequest()
    {
        if (channel.owner.isAdmin)
        {
            wantsBattlEyeLogs = !wantsBattlEyeLogs;
        }
    }

    [Obsolete]
    public void tellTerminalRelay(CSteamID steamID, string internalMessage)
    {
        ReceiveTerminalRelay(internalMessage);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellTerminalRelay")]
    public void ReceiveTerminalRelay(string internalMessage)
    {
        UnturnedLog.info(internalMessage);
    }

    [Obsolete]
    public void sendTerminalRelay(string internalMessage, string internalCategory, string displayCategory)
    {
        sendTerminalRelay(internalMessage);
    }

    public void sendTerminalRelay(string internalMessage)
    {
        SendTerminalRelay.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), internalMessage);
    }

    internal void PostTeleport()
    {
        onPlayerTeleported?.Invoke(this, base.transform.position);
        VolumeManager<CullingVolume, CullingVolumeManager>.Get().OnPlayerTeleported();
    }

    [Obsolete]
    public void askTeleport(CSteamID steamID, Vector3 position, byte angle)
    {
        ReceiveTeleport(position, angle);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askTeleport")]
    public void ReceiveTeleport(Vector3 position, byte angle)
    {
        bool flag = false;
        if (movement.controller != null)
        {
            movement.controller.DisableDetectCollisionsUntilNextFrame();
            flag = movement.controller.enabled;
            movement.controller.enabled = false;
        }
        base.transform.position = position + new Vector3(0f, 0.5f, 0f);
        base.transform.rotation = Quaternion.Euler(0f, angle * 2, 0f);
        if (flag)
        {
            movement.controller.enabled = true;
        }
        look.updateLook();
        movement.updateMovement();
        PostTeleport();
    }

    public void sendTeleport(Vector3 position, byte angle)
    {
        CommandWindow.LogWarning("Please use teleportToPlayer or teleportToLocation rather than sendTeleport, as they check for error conditions and safe space");
        teleportToLocation(position, (int)angle);
    }

    public bool teleportToPlayer(Player otherPlayer)
    {
        if (otherPlayer == null)
        {
            return false;
        }
        if (otherPlayer.movement.getVehicle() != null)
        {
            return false;
        }
        Vector3 position = otherPlayer.transform.position;
        float y = otherPlayer.transform.rotation.eulerAngles.y;
        return teleportToLocation(position, y);
    }

    public bool teleportToLocation(Vector3 position, float yaw)
    {
        if (!stance.wouldHaveHeightClearanceAtPosition(position, 0.5f))
        {
            return false;
        }
        teleportToLocationUnsafe(position, yaw);
        return true;
    }

    public bool teleportToRandomSpawnPoint()
    {
        PlayerSpawnpoint spawn = LevelPlayers.getSpawn(isAlt: false);
        if (spawn != null)
        {
            teleportToLocationUnsafe(spawn.point + new Vector3(0f, 0.5f, 0f), spawn.angle);
            return true;
        }
        return false;
    }

    public bool teleportToBed()
    {
        if (BarricadeManager.tryGetBed(channel.owner.playerID.steamID, out var point, out var angle))
        {
            point.y += 0.5f;
            float yaw = MeasurementTool.byteToAngle(angle);
            return teleportToLocation(point, yaw);
        }
        return false;
    }

    public bool adjustStanceOrTeleportIfStuck()
    {
        return stance.adjustStanceOrTeleportIfStuck();
    }

    public void teleportToLocationUnsafe(Vector3 position, float yaw)
    {
        InteractableVehicle vehicle = movement.getVehicle();
        if (vehicle == null)
        {
            byte b = MeasurementTool.angleToByte(yaw);
            if (movement.canAddSimulationResultsToUpdates)
            {
                SendTeleport.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), position, b);
                return;
            }
            SendTeleport.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), position, b);
            ReceiveTeleport(position, b);
        }
        else
        {
            VehicleManager.removePlayerTeleportUnsafe(vehicle, this, position, yaw);
        }
    }

    [Obsolete]
    public void tellStat(CSteamID steamID, byte newStat)
    {
        ReceiveStat((EPlayerStat)newStat);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellStat")]
    public void ReceiveStat(EPlayerStat stat)
    {
        if (stat == EPlayerStat.NONE)
        {
            return;
        }
        trackStat(stat);
        switch (stat)
        {
        case EPlayerStat.KILLS_PLAYERS:
        {
            if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Kills_Players", out int data6))
            {
                Provider.provider.statisticsService.userStatisticsService.setStatistic("Kills_Players", data6 + 1);
            }
            break;
        }
        case EPlayerStat.KILLS_ZOMBIES_NORMAL:
        {
            if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Kills_Zombies_Normal", out int data10))
            {
                Provider.provider.statisticsService.userStatisticsService.setStatistic("Kills_Zombies_Normal", data10 + 1);
            }
            break;
        }
        case EPlayerStat.KILLS_ZOMBIES_MEGA:
        {
            if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Kills_Zombies_Mega", out int data2))
            {
                Provider.provider.statisticsService.userStatisticsService.setStatistic("Kills_Zombies_Mega", data2 + 1);
            }
            break;
        }
        case EPlayerStat.FOUND_ITEMS:
        {
            if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Items", out int data8))
            {
                Provider.provider.statisticsService.userStatisticsService.setStatistic("Found_Items", data8 + 1);
            }
            break;
        }
        case EPlayerStat.FOUND_RESOURCES:
        {
            if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Resources", out int data4))
            {
                Provider.provider.statisticsService.userStatisticsService.setStatistic("Found_Resources", data4 + 1);
            }
            break;
        }
        case EPlayerStat.KILLS_ANIMALS:
        {
            if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Kills_Animals", out int data11))
            {
                Provider.provider.statisticsService.userStatisticsService.setStatistic("Kills_Animals", data11 + 1);
            }
            break;
        }
        case EPlayerStat.FOUND_CRAFTS:
        {
            if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Crafts", out int data9))
            {
                Provider.provider.statisticsService.userStatisticsService.setStatistic("Found_Crafts", data9 + 1);
            }
            break;
        }
        case EPlayerStat.FOUND_FISHES:
        {
            if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Fishes", out int data7))
            {
                Provider.provider.statisticsService.userStatisticsService.setStatistic("Found_Fishes", data7 + 1);
            }
            break;
        }
        case EPlayerStat.FOUND_PLANTS:
        {
            if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Plants", out int data5))
            {
                Provider.provider.statisticsService.userStatisticsService.setStatistic("Found_Plants", data5 + 1);
            }
            break;
        }
        case EPlayerStat.ARENA_WINS:
        {
            if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Arena_Wins", out int data3))
            {
                Provider.provider.statisticsService.userStatisticsService.setStatistic("Arena_Wins", data3 + 1);
            }
            break;
        }
        case EPlayerStat.FOUND_BUILDABLES:
        {
            if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Buildables", out int data))
            {
                Provider.provider.statisticsService.userStatisticsService.setStatistic("Found_Buildables", data + 1);
            }
            break;
        }
        }
    }

    [Obsolete]
    public void tellAchievementUnlocked(CSteamID steamID, string id)
    {
        ReceiveAchievementUnlocked(id);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellAchievementUnlocked")]
    public void ReceiveAchievementUnlocked(string id)
    {
        if (Provider.statusData.Achievements.canBeGrantedByNPC(id))
        {
            if (Provider.provider.achievementsService.getAchievement(id, out var has) && !has)
            {
                Provider.provider.achievementsService.setAchievement(id);
            }
        }
        else
        {
            UnturnedLog.warn("Achievement " + id + " cannot be unlocked by NPCs");
        }
    }

    protected void trackStat(EPlayerStat stat)
    {
        if (equipment.isSelected && equipment.isEquipped)
        {
            channel.owner.incrementStatTrackerValue(equipment.itemID, stat);
        }
    }

    public void sendStat(EPlayerKill kill)
    {
        switch (kill)
        {
        case EPlayerKill.PLAYER:
            sendStat(EPlayerStat.KILLS_PLAYERS);
            break;
        case EPlayerKill.ZOMBIE:
            sendStat(EPlayerStat.KILLS_ZOMBIES_NORMAL);
            break;
        case EPlayerKill.MEGA:
            sendStat(EPlayerStat.KILLS_ZOMBIES_MEGA);
            break;
        case EPlayerKill.ANIMAL:
            sendStat(EPlayerStat.KILLS_ANIMALS);
            break;
        case EPlayerKill.RESOURCE:
            sendStat(EPlayerStat.FOUND_RESOURCES);
            break;
        }
    }

    public void sendStat(EPlayerStat stat)
    {
        if (!channel.isOwner)
        {
            trackStat(stat);
        }
        Player.onPlayerStatIncremented?.Invoke(this, stat);
        SendStat.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), stat);
    }

    public void sendAchievementUnlocked(string id)
    {
        SendAchievementUnlocked.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), id);
    }

    [Obsolete]
    public void askMessage(CSteamID steamID, byte message)
    {
        ReceiveUIMessage((EPlayerMessage)message);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askMessage")]
    public void ReceiveUIMessage(EPlayerMessage message)
    {
        PlayerUI.message(message, string.Empty);
    }

    public void sendMessage(EPlayerMessage message)
    {
        SendUIMessage.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GetOwnerTransportConnection(), message);
    }

    public void enableItemSpotLight(PlayerSpotLightConfig config)
    {
        itemOn = true;
        itemLightConfig = config;
        updateLights();
    }

    public void disableItemSpotLight()
    {
        itemOn = false;
        updateLights();
    }

    public void updateGlassesLights(bool on)
    {
        if (clothing.firstClothes != null && clothing.firstClothes.glassesModel != null)
        {
            Transform transform = clothing.firstClothes.glassesModel.Find("Model_0");
            if (transform != null)
            {
                Transform transform2 = transform.Find("Light");
                if (transform2 != null)
                {
                    transform2.gameObject.SetActive(on);
                }
            }
        }
        if (clothing.thirdClothes != null && clothing.thirdClothes.glassesModel != null)
        {
            Transform transform3 = clothing.thirdClothes.glassesModel.Find("Model_0");
            if (transform3 != null)
            {
                Transform transform4 = transform3.Find("Light");
                if (transform4 != null)
                {
                    transform4.gameObject.SetActive(on);
                }
            }
        }
        if (!(clothing.characterClothes != null) || !(clothing.characterClothes.glassesModel != null))
        {
            return;
        }
        Transform transform5 = clothing.characterClothes.glassesModel.Find("Model_0");
        if (transform5 != null)
        {
            Transform transform6 = transform5.Find("Light");
            if (transform6 != null)
            {
                transform6.gameObject.SetActive(on);
            }
        }
    }

    public void enableHeadlamp(PlayerSpotLightConfig config)
    {
        headlampOn = true;
        headlampLightConfig = config;
        updateLights();
    }

    public void disableHeadlamp()
    {
        headlampOn = false;
        updateLights();
    }

    private void updateLights()
    {
        if (Dedicator.IsDedicatedServer)
        {
            return;
        }
        if (channel.isOwner)
        {
            firstSpot.gameObject.SetActive(isSpotOn && look.perspective == EPlayerPerspective.FIRST);
            thirdSpot.gameObject.SetActive(isSpotOn && look.perspective == EPlayerPerspective.THIRD);
        }
        else
        {
            thirdSpot.gameObject.SetActive(isSpotOn);
        }
        if (isSpotOn)
        {
            PlayerSpotLightConfig playerSpotLightConfig = lightConfig;
            if (firstSpot != null)
            {
                playerSpotLightConfig.applyToLight(firstSpot.GetComponent<Light>());
            }
            if (thirdSpot != null)
            {
                playerSpotLightConfig.applyToLight(thirdSpot.GetComponent<Light>());
            }
        }
    }

    private void onPerspectiveUpdated(EPlayerPerspective newPerspective)
    {
        if (isSpotOn)
        {
            updateLights();
        }
    }

    public bool tryToPerformRateLimitedAction()
    {
        bool num = rateLimitedActionsCredits < 1f;
        if (num)
        {
            rateLimitedActionsCredits += 1f / (float)maxRateLimitedActionsPerSecond;
        }
        return num;
    }

    protected void updateRateLimiting()
    {
        rateLimitedActionsCredits -= Time.deltaTime;
        if (rateLimitedActionsCredits < 0f)
        {
            rateLimitedActionsCredits = 0f;
        }
    }

    private void Update()
    {
        if (Provider.isServer)
        {
            updateRateLimiting();
        }
    }

    private void InitializePlayerStart()
    {
        if (channel.isOwner)
        {
            _player = this;
            _first = base.transform.Find("First");
            _third = base.transform.Find("Third");
            first.gameObject.SetActive(value: true);
            third.gameObject.SetActive(value: true);
            _character = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("Characters/Inspect"))).transform;
            character.name = "Inspect";
            character.transform.position = new Vector3(256f, -256f, 0f);
            character.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            firstSpot = MainCamera.instance.transform.Find("Spot");
            firstSpot.localPosition = Vector3.zero;
            isLoadingInventory = true;
            isLoadingLife = true;
            isLoadingClothing = true;
            PlayerLook playerLook = look;
            playerLook.onPerspectiveUpdated = (PerspectiveUpdated)Delegate.Combine(playerLook.onPerspectiveUpdated, new PerspectiveUpdated(onPerspectiveUpdated));
        }
        else
        {
            _first = null;
            _third = base.transform.Find("Third");
            third.gameObject.SetActive(value: true);
        }
        thirdSpot = third.Find("Skeleton").Find("Spine").Find("Skull")
            .Find("Spot");
    }

    internal void AssignNetIdBlock(NetId baseId)
    {
        _netId = ++baseId;
        NetIdRegistry.Assign(_netId, this);
        _animator.AssignNetId(++baseId);
        _clothing.AssignNetId(++baseId);
        _crafting.AssignNetId(++baseId);
        _equipment.AssignNetId(++baseId);
        _input.AssignNetId(++baseId);
        _interact.AssignNetId(++baseId);
        _inventory.AssignNetId(++baseId);
        _life.AssignNetId(++baseId);
        _look.AssignNetId(++baseId);
        _movement.AssignNetId(++baseId);
        _quests.AssignNetId(++baseId);
        _skills.AssignNetId(++baseId);
        _stance.AssignNetId(++baseId);
        _voice.AssignNetId(++baseId);
    }

    internal void InitializePlayer()
    {
        PlayerUI playerUI = null;
        if (channel.isOwner)
        {
            playerUI = base.transform.Find("First")?.Find("Camera")?.GetComponent<PlayerUI>();
        }
        InitializePlayerStart();
        clothing.InitializePlayer();
        inventory.InitializePlayer();
        life.InitializePlayer();
        skills.InitializePlayer();
        crafting.InitializePlayer();
        stance.InitializePlayer();
        movement.InitializePlayer();
        look.InitializePlayer();
        interact.InitializePlayer();
        animator.InitializePlayer();
        equipment.InitializePlayer();
        input.InitializePlayer();
        voice.InitializePlayer();
        if (workzone != null)
        {
            workzone.InitializePlayer();
        }
        quests.InitializePlayer();
        if (playerUI != null)
        {
            playerUI.InitializePlayer();
        }
    }

    internal void SendInitialPlayerState(SteamPlayer client)
    {
        clothing.SendInitialPlayerState(client);
        inventory.SendInitialPlayerState(client);
        life.SendInitialPlayerState(client);
        skills.SendInitialPlayerState(client);
        stance.SendInitialPlayerState(client);
        quests.SendInitialPlayerState(client);
        equipment.SendInitialPlayerState(client);
        animator.SendInitialPlayerState(client);
    }

    internal void SendInitialPlayerState(List<ITransportConnection> transportConnections)
    {
        clothing.SendInitialPlayerState(transportConnections);
        life.SendInitialPlayerState(transportConnections);
        skills.SendInitialPlayerState(transportConnections);
        stance.SendInitialPlayerState(transportConnections);
        quests.SendInitialPlayerState(transportConnections);
        equipment.SendInitialPlayerState(transportConnections);
        animator.SendInitialPlayerState(transportConnections);
    }

    internal void ReleaseNetIdBlock()
    {
        NetIdRegistry.Release(_netId);
        _netId.Clear();
        _animator.ReleaseNetId();
        _clothing.ReleaseNetId();
        _crafting.ReleaseNetId();
        _equipment.ReleaseNetId();
        _input.ReleaseNetId();
        _interact.ReleaseNetId();
        _inventory.ReleaseNetId();
        _life.ReleaseNetId();
        _look.ReleaseNetId();
        _movement.ReleaseNetId();
        _quests.ReleaseNetId();
        _skills.ReleaseNetId();
        _stance.ReleaseNetId();
        _voice.ReleaseNetId();
    }

    private void Awake()
    {
        _channel = GetComponent<SteamChannel>();
        agro = 0;
        _animator = GetComponent<PlayerAnimator>();
        _clothing = GetComponent<PlayerClothing>();
        _inventory = GetComponent<PlayerInventory>();
        _equipment = GetComponent<PlayerEquipment>();
        _life = GetComponent<PlayerLife>();
        _crafting = GetComponent<PlayerCrafting>();
        _skills = GetComponent<PlayerSkills>();
        _movement = GetComponent<PlayerMovement>();
        _look = GetComponent<PlayerLook>();
        _stance = GetComponent<PlayerStance>();
        _input = GetComponent<PlayerInput>();
        _voice = GetComponent<PlayerVoice>();
        _interact = GetComponent<PlayerInteract>();
        _workzone = GetComponent<PlayerWorkzone>();
        _quests = GetComponent<PlayerQuests>();
    }

    private void OnDestroy()
    {
        if (screenshotFinal != null)
        {
            UnityEngine.Object.DestroyImmediate(screenshotFinal);
            screenshotFinal = null;
        }
        if (channel != null && channel.isOwner)
        {
            isLoadingInventory = false;
            isLoadingLife = false;
            isLoadingClothing = false;
            channel.owner.commitModifiedDynamicProps();
        }
    }

    public void save()
    {
        savePositionAndRotation();
        clothing.save();
        inventory.save();
        life.save();
        skills.save();
        animator.save();
        quests.save();
    }

    protected void savePositionAndRotation()
    {
        bool flag = !life.isDead;
        if (flag)
        {
            Vector3 vector = base.transform.position;
            InteractableVehicle vehicle = movement.getVehicle();
            if (vehicle != null && vehicle.findPlayerSeat(this, out var seat) && vehicle.tryGetExit(seat, out var point, out var _))
            {
                vector = point;
            }
            flag = vector.IsFinite();
        }
        if (flag)
        {
            Block block = new Block();
            block.writeByte(SAVEDATA_VERSION);
            block.writeSingleVector3(base.transform.position);
            block.writeByte((byte)(base.transform.rotation.eulerAngles.y / 2f));
            PlayerSavedata.writeBlock(channel.owner.playerID, "/Player/Player.dat", block);
        }
        else if (PlayerSavedata.fileExists(channel.owner.playerID, "/Player/Player.dat"))
        {
            PlayerSavedata.deleteFile(channel.owner.playerID, "/Player/Player.dat");
        }
    }

    public NetId GetNetId()
    {
        return _netId;
    }
}
