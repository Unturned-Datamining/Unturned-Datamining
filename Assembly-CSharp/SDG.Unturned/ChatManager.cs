using System;
using System.Collections.Generic;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class ChatManager : SteamCaller
{
    public delegate void ClientUnityEventPermissionsHandler(SteamPlayer player, string command, ref bool shouldExecuteCommand, ref bool shouldList);

    public static readonly int MAX_MESSAGE_LENGTH = 127;

    public static ChatMessageReceivedHandler onChatMessageReceived;

    public static ServerSendingChatMessageHandler onServerSendingMessage;

    public static ServerFormattingChatMessageHandler onServerFormattingMessage;

    public static Chatted onChatted;

    public static CheckPermissions onCheckPermissions;

    public static VotingStart onVotingStart;

    public static VotingUpdate onVotingUpdate;

    public static VotingStop onVotingStop;

    public static VotingMessage onVotingMessage;

    public static string welcomeText = "";

    public static Color welcomeColor = Palette.SERVER;

    public static float chatrate = 0.25f;

    public static bool voteAllowed = false;

    public static float votePassCooldown = 5f;

    public static float voteFailCooldown = 60f;

    public static float voteDuration = 15f;

    public static float votePercentage = 0.75f;

    public static byte votePlayers = 3;

    private static float lastVote;

    private static bool isVoting;

    private static bool needsVote;

    private static bool hasVote;

    private static byte voteYes;

    private static byte voteNo;

    private static byte votesPossible;

    private static byte votesNeeded;

    private static SteamPlayer voteOrigin;

    private static CSteamID voteTarget;

    private static uint voteIP;

    private static List<CSteamID> votes;

    private static ChatManager manager;

    private static List<ReceivedChatMessage> _receivedChatHistory = new List<ReceivedChatMessage>();

    private static readonly ClientStaticMethod<CSteamID, CSteamID, byte> SendVoteStart = ClientStaticMethod<CSteamID, CSteamID, byte>.Get(ReceiveVoteStart);

    private static readonly ClientStaticMethod<byte, byte> SendVoteUpdate = ClientStaticMethod<byte, byte>.Get(ReceiveVoteUpdate);

    private static readonly ClientStaticMethod<EVotingMessage> SendVoteStop = ClientStaticMethod<EVotingMessage>.Get(ReceiveVoteStop);

    private static readonly ClientStaticMethod<EVotingMessage> SendVoteMessage = ClientStaticMethod<EVotingMessage>.Get(ReceiveVoteMessage);

    private static readonly ServerStaticMethod<bool> SendSubmitVoteRequest = ServerStaticMethod<bool>.Get(ReceiveSubmitVoteRequest);

    private static readonly ServerStaticMethod<CSteamID> SendCallVoteRequest = ServerStaticMethod<CSteamID>.Get(ReceiveCallVoteRequest);

    private static readonly ClientStaticMethod<CSteamID, string, EChatMode, Color, bool, string> SendChatEntry = ClientStaticMethod<CSteamID, string, EChatMode, Color, bool, string>.Get(ReceiveChatEntry);

    private static readonly ServerStaticMethod<byte, string> SendChatRequest = ServerStaticMethod<byte, string>.Get(ReceiveChatRequest);

    private static string[] recentlySentMessages = new string[10];

    public static ChatManager instance => manager;

    public static List<ReceivedChatMessage> receivedChatHistory => _receivedChatHistory;

    public static event ClientUnityEventPermissionsHandler onCheckUnityEventPermissions;

    public static void receiveChatMessage(CSteamID speakerSteamID, string iconURL, EChatMode mode, Color color, bool isRich, string text)
    {
        text = text.Trim();
        ControlsSettings.formatPluginHotkeysIntoText(ref text);
        if (OptionsSettings.filter)
        {
            ProfanityFilter.filter(ref text);
        }
        if (OptionsSettings.streamer)
        {
            color = Color.white;
        }
        SteamPlayer speaker;
        if (speakerSteamID == CSteamID.Nil)
        {
            speaker = null;
        }
        else
        {
            if (!OptionsSettings.chatText && speakerSteamID != Provider.client)
            {
                return;
            }
            speaker = PlayerTool.getSteamPlayer(speakerSteamID);
        }
        ReceivedChatMessage item = new ReceivedChatMessage(speaker, iconURL, mode, color, isRich, text);
        receivedChatHistory.Insert(0, item);
        if (receivedChatHistory.Count > Provider.preferenceData.Chat.History_Length)
        {
            receivedChatHistory.RemoveAt(receivedChatHistory.Count - 1);
        }
        if (onChatMessageReceived != null)
        {
            onChatMessageReceived();
        }
    }

    public static bool process(SteamPlayer player, string cmd)
    {
        return process(player, cmd, fromUnityEvent: false);
    }

    public static bool process(SteamPlayer player, string cmd, bool fromUnityEvent)
    {
        bool shouldExecuteCommand = false;
        bool shouldList = true;
        string text = cmd.Substring(0, 1);
        if (text == "@" || text == "/")
        {
            if (player.isAdmin)
            {
                shouldExecuteCommand = true;
                shouldList = false;
            }
            if (fromUnityEvent && !Provider.configData.UnityEvents.Allow_Client_Commands)
            {
                shouldExecuteCommand = false;
                shouldList = false;
            }
        }
        if (onCheckPermissions != null)
        {
            onCheckPermissions(player, cmd, ref shouldExecuteCommand, ref shouldList);
        }
        if (fromUnityEvent)
        {
            ChatManager.onCheckUnityEventPermissions?.Invoke(player, cmd, ref shouldExecuteCommand, ref shouldList);
        }
        if (shouldExecuteCommand)
        {
            Commander.execute(player.playerID.steamID, cmd.Substring(1));
        }
        return shouldList;
    }

    [Obsolete]
    public void tellVoteStart(CSteamID steamID, CSteamID origin, CSteamID target, byte votesNeeded)
    {
        ReceiveVoteStart(origin, target, votesNeeded);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVoteStart")]
    public static void ReceiveVoteStart(CSteamID origin, CSteamID target, byte votesNeeded)
    {
        SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(origin);
        if (steamPlayer == null)
        {
            return;
        }
        SteamPlayer steamPlayer2 = PlayerTool.getSteamPlayer(target);
        if (steamPlayer2 != null)
        {
            needsVote = true;
            hasVote = false;
            if (onVotingStart != null)
            {
                onVotingStart(steamPlayer, steamPlayer2, votesNeeded);
            }
        }
    }

    [Obsolete]
    public void tellVoteUpdate(CSteamID steamID, byte voteYes, byte voteNo)
    {
        ReceiveVoteUpdate(voteYes, voteNo);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVoteMessage")]
    public static void ReceiveVoteUpdate(byte voteYes, byte voteNo)
    {
        if (onVotingUpdate != null)
        {
            onVotingUpdate(voteYes, voteNo);
        }
    }

    [Obsolete]
    public void tellVoteStop(CSteamID steamID, byte message)
    {
        ReceiveVoteStop((EVotingMessage)message);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVoteStop")]
    public static void ReceiveVoteStop(EVotingMessage message)
    {
        needsVote = false;
        if (onVotingStop != null)
        {
            onVotingStop(message);
        }
    }

    [Obsolete]
    public void tellVoteMessage(CSteamID steamID, byte message)
    {
        ReceiveVoteMessage((EVotingMessage)message);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVoteMessage")]
    public static void ReceiveVoteMessage(EVotingMessage message)
    {
        if (onVotingMessage != null)
        {
            onVotingMessage(message);
        }
    }

    [Obsolete]
    public void askVote(CSteamID steamID, bool vote)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveSubmitVoteRequest(in context, vote);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2, legacyName = "askVote")]
    public static void ReceiveSubmitVoteRequest(in ServerInvocationContext context, bool vote)
    {
        SteamPlayer callingPlayer = context.GetCallingPlayer();
        if (callingPlayer != null && isVoting && !votes.Contains(callingPlayer.playerID.steamID))
        {
            votes.Add(callingPlayer.playerID.steamID);
            if (vote)
            {
                voteYes++;
            }
            else
            {
                voteNo++;
            }
            SendVoteUpdate.Invoke(ENetReliability.Reliable, Provider.EnumerateClients(), voteYes, voteNo);
        }
    }

    [Obsolete]
    public void askCallVote(CSteamID steamID, CSteamID target)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveCallVoteRequest(in context, target);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2, legacyName = "askCallVote")]
    public static void ReceiveCallVoteRequest(in ServerInvocationContext context, CSteamID target)
    {
        if (isVoting)
        {
            return;
        }
        SteamPlayer callingPlayer = context.GetCallingPlayer();
        if (callingPlayer == null || Time.realtimeSinceStartup < callingPlayer.nextVote)
        {
            SendVoteMessage.Invoke(ENetReliability.Reliable, context.GetTransportConnection(), EVotingMessage.DELAY);
            return;
        }
        if (!voteAllowed)
        {
            SendVoteMessage.Invoke(ENetReliability.Reliable, context.GetTransportConnection(), EVotingMessage.OFF);
            return;
        }
        SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(target);
        if (steamPlayer != null && !steamPlayer.isAdmin)
        {
            if (Provider.clients.Count < votePlayers)
            {
                SendVoteMessage.Invoke(ENetReliability.Reliable, context.GetTransportConnection(), EVotingMessage.PLAYERS);
                return;
            }
            CommandWindow.Log(Provider.localization.format("Vote_Kick", callingPlayer.playerID.characterName, callingPlayer.playerID.playerName, steamPlayer.playerID.characterName, steamPlayer.playerID.playerName));
            lastVote = Time.realtimeSinceStartup;
            isVoting = true;
            voteYes = 0;
            voteNo = 0;
            votesPossible = (byte)Provider.clients.Count;
            votesNeeded = (byte)Mathf.Ceil((float)(int)votesPossible * votePercentage);
            voteOrigin = callingPlayer;
            voteTarget = target;
            votes = new List<CSteamID>();
            voteIP = steamPlayer.getIPv4AddressOrZero();
            SendVoteStart.Invoke(ENetReliability.Reliable, Provider.EnumerateClients(), callingPlayer.playerID.steamID, target, votesNeeded);
        }
    }

    public static void sendVote(bool vote)
    {
        SendSubmitVoteRequest.Invoke(ENetReliability.Reliable, vote);
    }

    public static void sendCallVote(CSteamID target)
    {
        SendCallVoteRequest.Invoke(ENetReliability.Unreliable, target);
    }

    [Obsolete]
    public void tellChat(CSteamID steamID, CSteamID owner, string iconURL, byte mode, Color color, bool rich, string text)
    {
        ReceiveChatEntry(owner, iconURL, (EChatMode)mode, color, rich, text);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellChat")]
    public static void ReceiveChatEntry(CSteamID owner, string iconURL, EChatMode mode, Color color, bool rich, string text)
    {
        receiveChatMessage(owner, iconURL, mode, color, rich, text);
    }

    [Obsolete]
    public void askChat(CSteamID steamID, byte flags, string text)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveChatRequest(in context, flags, text);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 15, legacyName = "askChat")]
    public static void ReceiveChatRequest(in ServerInvocationContext context, byte flags, string text)
    {
        SteamPlayer callingPlayer = context.GetCallingPlayer();
        if (callingPlayer == null || callingPlayer.player == null || Time.realtimeSinceStartup - callingPlayer.lastChat < chatrate)
        {
            return;
        }
        callingPlayer.lastChat = Time.realtimeSinceStartup;
        EChatMode eChatMode = (EChatMode)(flags & 0x7F);
        bool flag = (flags & 0x80) > 0;
        if (text.Length < 2 || (flag && !Provider.configData.UnityEvents.Allow_Client_Messages))
        {
            return;
        }
        text = text.Trim();
        if (text.Length < 2)
        {
            return;
        }
        if (text.Length > MAX_MESSAGE_LENGTH)
        {
            text = text.Substring(0, MAX_MESSAGE_LENGTH);
        }
        switch (eChatMode)
        {
        case EChatMode.GLOBAL:
            if (CommandWindow.shouldLogChat)
            {
                CommandWindow.Log(Provider.localization.format("Global", callingPlayer.playerID.characterName, callingPlayer.playerID.playerName, text));
            }
            break;
        case EChatMode.LOCAL:
            if (CommandWindow.shouldLogChat)
            {
                CommandWindow.Log(Provider.localization.format("Local", callingPlayer.playerID.characterName, callingPlayer.playerID.playerName, text));
            }
            break;
        case EChatMode.GROUP:
            if (CommandWindow.shouldLogChat)
            {
                CommandWindow.Log(Provider.localization.format("Group", callingPlayer.playerID.characterName, callingPlayer.playerID.playerName, text));
            }
            break;
        default:
            return;
        }
        if (flag)
        {
            UnturnedLog.info("UnityEventMsg {0}: '{1}'", callingPlayer.playerID.steamID, text);
        }
        Color chatted = Color.white;
        if (callingPlayer.isAdmin && !Provider.hideAdmins)
        {
            chatted = Palette.ADMIN;
        }
        else if (callingPlayer.isPro)
        {
            chatted = Palette.PRO;
        }
        bool isRich = false;
        bool isVisible = true;
        if (onChatted != null)
        {
            onChatted(callingPlayer, eChatMode, ref chatted, ref isRich, text, ref isVisible);
        }
        if (!(process(callingPlayer, text, flag) && isVisible))
        {
            return;
        }
        if (onServerFormattingMessage != null)
        {
            onServerFormattingMessage(callingPlayer, eChatMode, ref text);
        }
        else
        {
            text = "%SPEAKER%: " + text;
            switch (eChatMode)
            {
            case EChatMode.LOCAL:
                text = "[A] " + text;
                break;
            case EChatMode.GROUP:
                text = "[G] " + text;
                break;
            }
        }
        switch (eChatMode)
        {
        case EChatMode.GLOBAL:
            serverSendMessage(text, chatted, callingPlayer, null, EChatMode.GLOBAL, null, isRich);
            break;
        case EChatMode.LOCAL:
        {
            float num = 16384f;
            {
                foreach (SteamPlayer client in Provider.clients)
                {
                    if (!(client.player == null) && (client.player.transform.position - callingPlayer.player.transform.position).sqrMagnitude < num)
                    {
                        serverSendMessage(text, chatted, callingPlayer, client, EChatMode.LOCAL, null, isRich);
                    }
                }
                break;
            }
        }
        case EChatMode.GROUP:
            if (!(callingPlayer.player.quests.groupID != CSteamID.Nil))
            {
                break;
            }
            {
                foreach (SteamPlayer client2 in Provider.clients)
                {
                    if (!(client2.player == null) && client2.player.quests.isMemberOfSameGroupAs(callingPlayer.player))
                    {
                        serverSendMessage(text, chatted, callingPlayer, client2, EChatMode.GROUP, null, isRich);
                    }
                }
                break;
            }
        }
    }

    public static string getRecentlySentMessage(int index)
    {
        index %= recentlySentMessages.Length;
        return recentlySentMessages[index];
    }

    public static void sendChat(EChatMode mode, string text)
    {
        for (int num = recentlySentMessages.Length - 1; num > 0; num--)
        {
            recentlySentMessages[num] = recentlySentMessages[num - 1];
        }
        recentlySentMessages[0] = text;
        SendChatRequest.Invoke(ENetReliability.Reliable, (byte)mode, text);
    }

    public static void clientSendMessage_UnityEvent(EChatMode mode, string text, ClientTextChatMessenger messenger)
    {
        if (messenger == null)
        {
            throw new ArgumentNullException("messenger");
        }
        UnturnedLog.info("UnityEventMsg {0}: '{1}'", messenger.gameObject.GetSceneHierarchyPath(), text);
        SendChatRequest.Invoke(ENetReliability.Reliable, (byte)(mode | (EChatMode)128), text);
    }

    public static void serverSendMessage_UnityEvent(string text, Color color, string iconURL, bool useRichTextFormatting, ServerTextChatMessenger messenger)
    {
        if (messenger == null)
        {
            throw new ArgumentNullException("messenger");
        }
        if (Provider.configData.UnityEvents.Allow_Server_Messages)
        {
            UnturnedLog.info("UnityEventMsg {0}: '{1}'", messenger.gameObject.GetSceneHierarchyPath(), text);
            serverSendMessage(text, color, null, null, EChatMode.SAY, iconURL, useRichTextFormatting);
        }
    }

    public static void say(CSteamID target, string text, Color color, bool isRich = false)
    {
        say(target, text, color, EChatMode.WELCOME, isRich);
    }

    public static void say(CSteamID target, string text, Color color, EChatMode mode, bool isRich = false)
    {
        SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(target);
        if (steamPlayer != null)
        {
            serverSendMessage(text, color, null, steamPlayer, EChatMode.SAY, null, isRich);
        }
    }

    public static void say(string text, Color color, bool isRich = false)
    {
        serverSendMessage(text, color, null, null, EChatMode.SAY, null, isRich);
    }

    public static void serverSendMessage(string text, Color color, SteamPlayer fromPlayer = null, SteamPlayer toPlayer = null, EChatMode mode = EChatMode.SAY, string iconURL = null, bool useRichTextFormatting = false)
    {
        if (!Provider.isServer)
        {
            throw new Exception("Tried to send server message, but currently a client! Text: " + text);
        }
        ThreadUtil.assertIsGameThread();
        if (onServerSendingMessage != null)
        {
            onServerSendingMessage(ref text, ref color, fromPlayer, toPlayer, mode, ref iconURL, ref useRichTextFormatting);
        }
        if (fromPlayer != null && toPlayer != null)
        {
            string newValue = ((string.IsNullOrEmpty(fromPlayer.playerID.nickName) || fromPlayer == toPlayer || !(toPlayer.player != null) || !(fromPlayer.player != null) || !fromPlayer.player.quests.isMemberOfSameGroupAs(toPlayer.player)) ? fromPlayer.playerID.characterName : fromPlayer.playerID.nickName);
            text = text.Replace("%SPEAKER%", newValue);
        }
        if (iconURL == null)
        {
            iconURL = string.Empty;
        }
        CSteamID arg = fromPlayer?.playerID.steamID ?? CSteamID.Nil;
        if (toPlayer == null)
        {
            foreach (SteamPlayer client in Provider.clients)
            {
                if (client != null)
                {
                    serverSendMessage(text, color, fromPlayer, client, mode, iconURL, useRichTextFormatting);
                }
            }
            return;
        }
        SendChatEntry.Invoke(ENetReliability.Reliable, toPlayer.transportConnection, arg, iconURL, mode, color, useRichTextFormatting, text);
    }

    private void onLevelLoaded(int level)
    {
        if (level > Level.BUILD_INDEX_SETUP)
        {
            receivedChatHistory.Clear();
        }
    }

    private void onServerConnected(CSteamID steamID)
    {
        if (Provider.isServer && welcomeText != "")
        {
            SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(steamID);
            say(steamPlayer.playerID.steamID, string.Format(welcomeText, steamPlayer.playerID.characterName), welcomeColor);
        }
    }

    private void Update()
    {
        if (isVoting && (Time.realtimeSinceStartup - lastVote > voteDuration || voteYes >= votesNeeded || voteNo > votesPossible - votesNeeded))
        {
            isVoting = false;
            if (voteYes >= votesNeeded)
            {
                if (voteOrigin != null)
                {
                    voteOrigin.nextVote = Time.realtimeSinceStartup + votePassCooldown;
                }
                CommandWindow.Log(Provider.localization.format("Vote_Pass"));
                SendVoteStop.Invoke(ENetReliability.Reliable, Provider.EnumerateClients(), EVotingMessage.PASS);
                SteamBlacklist.ban(voteTarget, voteIP, null, CSteamID.Nil, "you were vote kicked", SteamBlacklist.TEMPORARY);
            }
            else
            {
                if (voteOrigin != null)
                {
                    voteOrigin.nextVote = Time.realtimeSinceStartup + voteFailCooldown;
                }
                CommandWindow.Log(Provider.localization.format("Vote_Fail"));
                SendVoteStop.Invoke(ENetReliability.Reliable, Provider.EnumerateClients(), EVotingMessage.FAIL);
            }
        }
        if (needsVote && !hasVote)
        {
            if (InputEx.GetKeyDown(KeyCode.F1))
            {
                needsVote = false;
                hasVote = true;
                sendVote(vote: true);
            }
            else if (InputEx.GetKeyDown(KeyCode.F2))
            {
                needsVote = false;
                hasVote = true;
                sendVote(vote: false);
            }
        }
    }

    private void Start()
    {
        manager = this;
        Level.onLevelLoaded = (LevelLoaded)Delegate.Combine(Level.onLevelLoaded, new LevelLoaded(onLevelLoaded));
        Provider.onServerConnected = (Provider.ServerConnected)Delegate.Combine(Provider.onServerConnected, new Provider.ServerConnected(onServerConnected));
    }
}
