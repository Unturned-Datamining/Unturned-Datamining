using Steamworks;

namespace SDG.Unturned;

public static class Lobbies
{
    private static CallResult<LobbyCreated_t> lobbyCreated;

    private static Callback<LobbyEnter_t> lobbyEnter;

    private static Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;

    private static Callback<PersonaStateChange_t> personaStateChange;

    private static Callback<LobbyGameCreated_t> lobbyGameCreated;

    private static Callback<LobbyChatUpdate_t> lobbyChatUpdate;

    public static bool canOpenInvitations => SteamUtils.IsOverlayEnabled();

    public static bool isHost { get; private set; }

    public static bool inLobby { get; private set; }

    public static CSteamID currentLobby { get; private set; }

    public static event LobbiesRefreshedHandler lobbiesRefreshed;

    public static event LobbiesEnteredHandler lobbiesEntered;

    private static void onLobbyCreated(LobbyCreated_t callback, bool io)
    {
        UnturnedLog.info("Lobby created: {0} {1} {2}", callback.m_eResult, callback.m_ulSteamIDLobby, io);
        isHost = true;
    }

    private static void onLobbyEnter(LobbyEnter_t callback)
    {
        UnturnedLog.info("Lobby entered: {0} {1} {2} {3}", callback.m_bLocked, callback.m_ulSteamIDLobby, callback.m_EChatRoomEnterResponse, callback.m_rgfChatPermissions);
        inLobby = true;
        currentLobby = new CSteamID(callback.m_ulSteamIDLobby);
        triggerLobbiesRefreshed();
        triggerLobbiesEntered();
    }

    private static void onGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        UnturnedLog.info("Lobby join requested: {0} {1}", callback.m_steamIDLobby, callback.m_steamIDFriend);
        if (!Provider.isConnected)
        {
            joinLobby(callback.m_steamIDLobby);
        }
    }

    private static void onPersonaStateChange(PersonaStateChange_t callback)
    {
        if (!(currentLobby == CSteamID.Nil))
        {
            triggerLobbiesRefreshed();
        }
    }

    private static void onLobbyGameCreated(LobbyGameCreated_t callback)
    {
        UnturnedLog.info("Lobby game created: {0} {1} {2} {3}", callback.m_ulSteamIDLobby, callback.m_unIP, callback.m_usPort, callback.m_ulSteamIDGameServer);
        Provider.provider.matchmakingService.connect(new SteamConnectionInfo(callback.m_unIP, callback.m_usPort, ""));
        Provider.provider.matchmakingService.autoJoinServerQuery = true;
    }

    private static void onLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        UnturnedLog.info("Lobby chat update: {0} {1} {2} {3}", callback.m_ulSteamIDLobby, callback.m_ulSteamIDMakingChange, callback.m_ulSteamIDUserChanged, callback.m_rgfChatMemberStateChange);
        triggerLobbiesRefreshed();
    }

    private static void triggerLobbiesRefreshed()
    {
        Provider.updateRichPresence();
        Lobbies.lobbiesRefreshed?.Invoke();
    }

    private static void triggerLobbiesEntered()
    {
        Lobbies.lobbiesEntered?.Invoke();
    }

    public static void createLobby()
    {
        UnturnedLog.info("Create lobby");
        SteamAPICall_t hAPICall = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, 24);
        lobbyCreated.Set(hAPICall);
    }

    public static void joinLobby(CSteamID newLobby)
    {
        if (inLobby)
        {
            leaveLobby();
        }
        UnturnedLog.info("Join lobby: {0}", newLobby);
        SteamMatchmaking.JoinLobby(newLobby);
    }

    public static void LinkLobby(uint ip, ushort queryPort)
    {
        if (isHost)
        {
            UnturnedLog.info("Link lobby: {0} {1}", ip, queryPort);
            SteamMatchmaking.SetLobbyGameServer(currentLobby, ip, queryPort, CSteamID.Nil);
        }
    }

    public static void leaveLobby()
    {
        if (inLobby)
        {
            UnturnedLog.info("Leave lobby");
            isHost = false;
            inLobby = false;
            SteamMatchmaking.LeaveLobby(currentLobby);
        }
    }

    public static int getLobbyMemberCount()
    {
        return SteamMatchmaking.GetNumLobbyMembers(currentLobby);
    }

    public static CSteamID getLobbyMemberByIndex(int index)
    {
        return SteamMatchmaking.GetLobbyMemberByIndex(currentLobby, index);
    }

    public static void openInvitations()
    {
        SteamFriends.ActivateGameOverlayInviteDialog(currentLobby);
    }

    static Lobbies()
    {
        lobbyCreated = CallResult<LobbyCreated_t>.Create(onLobbyCreated);
        lobbyEnter = Callback<LobbyEnter_t>.Create(onLobbyEnter);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(onGameLobbyJoinRequested);
        personaStateChange = Callback<PersonaStateChange_t>.Create(onPersonaStateChange);
        lobbyGameCreated = Callback<LobbyGameCreated_t>.Create(onLobbyGameCreated);
        lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(onLobbyChatUpdate);
    }
}
