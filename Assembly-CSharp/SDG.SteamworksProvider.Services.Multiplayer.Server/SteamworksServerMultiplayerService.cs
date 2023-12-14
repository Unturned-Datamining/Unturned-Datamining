using System;
using System.IO;
using SDG.Provider.Services;
using SDG.Provider.Services.Community;
using SDG.Provider.Services.Multiplayer;
using SDG.Provider.Services.Multiplayer.Server;
using SDG.SteamworksProvider.Services.Community;
using SDG.Unturned;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Multiplayer.Server;

public class SteamworksServerMultiplayerService : Service, IServerMultiplayerService, IService
{
    private SteamworksAppInfo appInfo;

    private static Callback<SteamServerConnectFailure_t> steamServerConnectFailure;

    private static Callback<SteamServersConnected_t> steamServersConnected;

    private static Callback<SteamServersDisconnected_t> steamServersDisconnected;

    private static CommandLineFlag clShouldLogin = new CommandLineFlag(defaultValue: true, "-NoSteamGameServerLogin");

    private static CommandLineFlag clShouldEnableAdvertisement = new CommandLineFlag(defaultValue: true, "-NoSteamGameServerAdvertisement");

    public IServerInfo serverInfo { get; protected set; }

    public bool isHosting { get; protected set; }

    public MemoryStream stream { get; protected set; }

    public BinaryReader reader { get; protected set; }

    public BinaryWriter writer { get; protected set; }

    public event ServerMultiplayerServiceReadyHandler ready;

    public void open(uint ip, ushort port, ESecurityMode security)
    {
        if (isHosting)
        {
            return;
        }
        EServerMode eServerMode = EServerMode.eServerModeInvalid;
        switch (security)
        {
        case ESecurityMode.LAN:
            eServerMode = EServerMode.eServerModeNoAuthentication;
            break;
        case ESecurityMode.SECURE:
            eServerMode = EServerMode.eServerModeAuthenticationAndSecure;
            break;
        case ESecurityMode.INSECURE:
            eServerMode = EServerMode.eServerModeAuthentication;
            break;
        }
        ushort usGamePort = port;
        ushort usQueryPort = port;
        if (!GameServer.Init(ip, usGamePort, usQueryPort, eServerMode, "1.0.0.0"))
        {
            throw new Exception("GameServer API initialization failed!");
        }
        SteamGameServer.SetDedicatedServer(appInfo.isDedicated);
        SteamGameServer.SetProduct(appInfo.name);
        SteamGameServer.SetModDir(appInfo.name);
        if (SDG.Unturned.Provider.configData.Server.Experimental_Use_FakeIP)
        {
            if (!SteamGameServerNetworkingSockets.BeginAsyncRequestFakeIP(1))
            {
                CommandWindow.LogError("Fatal: BeginAsyncRequestFakeIP returned false");
                throw new NotSupportedException("BeginAsyncRequestFakeIP returned false");
            }
            CommandWindow.Log("Requesting \"FakeIP\" from Steam");
        }
        if ((bool)clShouldLogin)
        {
            string text = CommandGSLT.loginToken?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                text = SDG.Unturned.Provider.configData.Browser.Login_Token?.Trim();
            }
            if (string.IsNullOrEmpty(text))
            {
                UnturnedLog.info("Not using login token");
                if (security != 0)
                {
                    Level.onPostLevelLoaded = (PostLevelLoaded)Delegate.Combine(Level.onPostLevelLoaded, new PostLevelLoaded(OnPostLevelLoaded));
                }
                SteamGameServer.LogOnAnonymous();
            }
            else
            {
                if (text.Length == 32)
                {
                    UnturnedLog.info("Using login token");
                }
                else
                {
                    UnturnedLog.warn("Using login token, but it does not seem to be correctly formatted");
                }
                SteamGameServer.LogOn(text);
            }
        }
        else
        {
            UnturnedLog.info("Skipping Steam game server login");
        }
        if ((bool)clShouldEnableAdvertisement)
        {
            SteamGameServer.SetAdvertiseServerActive(bActive: true);
        }
        else
        {
            UnturnedLog.info("Not enabling Steam game server advertisement");
        }
        isHosting = true;
    }

    public void close()
    {
        if (isHosting)
        {
            SteamGameServer.SetAdvertiseServerActive(bActive: false);
            SteamGameServer.LogOff();
            GameServer.Shutdown();
            isHosting = false;
        }
    }

    public bool read(out ICommunityEntity entity, byte[] data, out ulong length, int channel)
    {
        entity = SteamworksCommunityEntity.INVALID;
        length = 0uL;
        return false;
    }

    public void write(ICommunityEntity entity, byte[] data, ulong length)
    {
    }

    public void write(ICommunityEntity entity, byte[] data, ulong length, ESendMethod method, int channel)
    {
    }

    public SteamworksServerMultiplayerService(SteamworksAppInfo newAppInfo)
    {
        appInfo = newAppInfo;
        steamServerConnectFailure = Callback<SteamServerConnectFailure_t>.CreateGameServer(onSteamServerConnectFailure);
        steamServersConnected = Callback<SteamServersConnected_t>.CreateGameServer(onSteamServersConnected);
        steamServersDisconnected = Callback<SteamServersDisconnected_t>.CreateGameServer(onSteamServersDisconnected);
    }

    private void onSteamServerConnectFailure(SteamServerConnectFailure_t callback)
    {
        if (!Dedicator.offlineOnly)
        {
            if (callback.m_bStillRetrying)
            {
                CommandWindow.LogFormat("Failed to connect to Steam servers because {0}, still retrying", callback.m_eResult);
            }
            else
            {
                CommandWindow.LogFormat("Failed to connect to Steam servers because {0}, no longer retrying", callback.m_eResult);
            }
            if (callback.m_eResult == EResult.k_EResultInvalidParam || callback.m_eResult == EResult.k_EResultAccountNotFound)
            {
                CommandWindow.LogWarning($"{callback.m_eResult} probably means Game Server Login Token (GSLT) is invalid");
            }
        }
    }

    private void onSteamServersConnected(SteamServersConnected_t callback)
    {
        this.ready?.Invoke();
    }

    private void onSteamServersDisconnected(SteamServersDisconnected_t callback)
    {
        if (!Dedicator.offlineOnly)
        {
            CommandWindow.LogFormat("Lost connection to Steam servers because {0}", callback.m_eResult);
        }
    }

    private void OnPostLevelLoaded(int id)
    {
        CommandWindow.LogWarning("Steam Game Server Login Token (GSLT) not set");
        CommandWindow.LogWarning("Without a login token the server:");
        CommandWindow.LogWarning("- Is not visible in Internet server list");
        CommandWindow.LogWarning("- Cannot be joined over the Internet");
        CommandWindow.LogWarning("See this link for guide and more information:");
        CommandWindow.LogWarning("https://docs.smartlydressedgames.com/en/stable/servers/game-server-login-tokens.html");
    }
}
