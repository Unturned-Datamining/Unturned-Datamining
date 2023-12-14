using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SDG.Framework.Utilities;
using SDG.Unturned;
using Steamworks;
using Unturned.SystemEx;

namespace SDG.NetTransport.SteamNetworkingSockets;

public class ServerTransport_SteamNetworkingSockets : TransportBase_SteamNetworkingSockets, IServerTransport
{
    private Callback<SteamNetworkingFakeIPResult_t> steamNetworkingFakeIpResultCallback;

    private Callback<SteamNetConnectionStatusChangedCallback_t> steamNetConnectionStatusChanged;

    private Callback<SteamNetAuthenticationStatus_t> steamNetAuthenticationStatusChanged;

    private ServerTransportConnectionFailureCallback connectionFailureCallback;

    private HSteamListenSocket ipListenSocket;

    private HSteamListenSocket fakeIpListenSocket;

    private HSteamListenSocket p2pListenSocket;

    private HSteamNetPollGroup pollGroup;

    private List<TransportConnection_SteamNetworkingSockets> transportConnections = new List<TransportConnection_SteamNetworkingSockets>();

    private IntPtr[] messageAddresses = new IntPtr[1];

    private bool didSetupDebugOutput;

    public void Initialize(ServerTransportConnectionFailureCallback connectionFailureCallback)
    {
        this.connectionFailureCallback = connectionFailureCallback;
        steamNetConnectionStatusChanged = Callback<SteamNetConnectionStatusChangedCallback_t>.CreateGameServer(OnSteamNetConnectionStatusChanged);
        steamNetAuthenticationStatusChanged = Callback<SteamNetAuthenticationStatus_t>.CreateGameServer(OnSteamNetAuthenticationStatusChanged);
        ESteamNetworkingSocketsDebugOutputType eSteamNetworkingSocketsDebugOutputType = SelectDebugOutputDetailLevel();
        if (eSteamNetworkingSocketsDebugOutputType != 0)
        {
            didSetupDebugOutput = true;
            Log("Server set SNS debug output detail level to {0}", eSteamNetworkingSocketsDebugOutputType);
            Steamworks.SteamGameServerNetworkingUtils.SetDebugOutputFunction(eSteamNetworkingSocketsDebugOutputType, GetDebugOutputFunction());
        }
        TimeUtility.updated += OnUpdate;
        CommandLogMemoryUsage.OnExecuted = (Action<List<string>>)Delegate.Combine(CommandLogMemoryUsage.OnExecuted, new Action<List<string>>(OnLogMemoryUsage));
        SteamNetworkingConfigValue_t[] array = BuildDefaultConfig().ToArray();
        SteamNetworkingIPAddr localAddress = default(SteamNetworkingIPAddr);
        if (string.IsNullOrEmpty(SDG.Unturned.Provider.bindAddress))
        {
            localAddress.Clear();
        }
        else if (SDG.Unturned.Provider.ip != 0)
        {
            localAddress.SetIPv4(SDG.Unturned.Provider.ip, 0);
        }
        else
        {
            Log("Unable to parse \"{0}\" as listen bind address", SDG.Unturned.Provider.bindAddress);
            localAddress.Clear();
        }
        localAddress.m_port = SDG.Unturned.Provider.GetServerConnectionPort();
        ipListenSocket = SteamGameServerNetworkingSockets.CreateListenSocketIP(ref localAddress, array.Length, array);
        Log("Server listen socket bound to {0}", AddressToString(localAddress));
        if (SDG.Unturned.Provider.configData.Server.Use_FakeIP)
        {
            fakeIpListenSocket = SteamGameServerNetworkingSockets.CreateListenSocketP2PFakeIP(0, array.Length, array);
            Log("Server FakeIP listen socket: {0}", fakeIpListenSocket);
            SteamGameServerNetworkingSockets.GetFakeIP(0, out var pInfo);
            if (pInfo.m_eResult == EResult.k_EResultBusy)
            {
                steamNetworkingFakeIpResultCallback = Callback<SteamNetworkingFakeIPResult_t>.CreateGameServer(OnSteamNetworkingFakeIpResultCallback);
                Log("Waiting for FakeIP callback...");
            }
            else
            {
                OnSteamNetworkingFakeIpResultCallback(pInfo);
            }
        }
        p2pListenSocket = SteamGameServerNetworkingSockets.CreateListenSocketP2P(0, array.Length, array);
        Log("Server P2P listen socket: {0}", p2pListenSocket);
        pollGroup = SteamGameServerNetworkingSockets.CreatePollGroup();
    }

    public void TearDown()
    {
        TimeUtility.updated -= OnUpdate;
        CommandLogMemoryUsage.OnExecuted = (Action<List<string>>)Delegate.Remove(CommandLogMemoryUsage.OnExecuted, new Action<List<string>>(OnLogMemoryUsage));
        steamNetConnectionStatusChanged.Dispose();
        steamNetAuthenticationStatusChanged.Dispose();
        steamNetworkingFakeIpResultCallback?.Dispose();
        if (!SteamGameServerNetworkingSockets.CloseListenSocket(ipListenSocket))
        {
            Log("Server failed to close IP listen socket {0}", ipListenSocket);
        }
        if (fakeIpListenSocket != HSteamListenSocket.Invalid)
        {
            bool flag = SteamGameServerNetworkingSockets.CloseListenSocket(fakeIpListenSocket);
            if (!flag)
            {
                Log("Server failed to close \"Fake IP\" listen socket {0}", flag);
            }
        }
        if (!SteamGameServerNetworkingSockets.CloseListenSocket(p2pListenSocket))
        {
            Log("Server failed to close P2P listen socket {0}", p2pListenSocket);
        }
        if (!SteamGameServerNetworkingSockets.DestroyPollGroup(pollGroup))
        {
            Log("Server failed to destroy poll group {0}", pollGroup);
        }
        if (didSetupDebugOutput)
        {
            didSetupDebugOutput = false;
            Steamworks.SteamGameServerNetworkingUtils.SetDebugOutputFunction(ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_None, null);
        }
    }

    public bool Receive(byte[] buffer, out long size, out ITransportConnection transportConnection)
    {
        while (SteamGameServerNetworkingSockets.ReceiveMessagesOnPollGroup(pollGroup, messageAddresses, messageAddresses.Length) >= 1)
        {
            IntPtr intPtr = messageAddresses[0];
            SteamNetworkingMessage_t steamNetworkingMessage_t = Marshal.PtrToStructure<SteamNetworkingMessage_t>(intPtr);
            if (steamNetworkingMessage_t.m_pData == IntPtr.Zero || steamNetworkingMessage_t.m_cbSize < 1)
            {
                SteamNetworkingMessage_t.Release(intPtr);
                continue;
            }
            TransportConnection_SteamNetworkingSockets transportConnection_SteamNetworkingSockets = FindConnection(steamNetworkingMessage_t.m_conn);
            if (transportConnection_SteamNetworkingSockets == null || transportConnection_SteamNetworkingSockets.wasClosed)
            {
                SteamNetworkingMessage_t.Release(intPtr);
                continue;
            }
            transportConnection = transportConnection_SteamNetworkingSockets;
            size = steamNetworkingMessage_t.m_cbSize;
            if (size > buffer.Length)
            {
                size = buffer.Length;
            }
            Marshal.Copy(steamNetworkingMessage_t.m_pData, buffer, 0, (int)size);
            SteamNetworkingMessage_t.Release(intPtr);
            return true;
        }
        size = 0L;
        transportConnection = null;
        return false;
    }

    internal void CloseConnection(TransportConnection_SteamNetworkingSockets transportConnection)
    {
        if (!transportConnection.wasClosed)
        {
            transportConnection.wasClosed = true;
            SteamGameServerNetworkingSockets.CloseConnection(transportConnection.steamConnectionHandle, 0, null, bEnableLinger: true);
            transportConnections.RemoveFast(transportConnection);
        }
    }

    /// <summary>
    /// Find game connection associated with Steam connection.
    /// </summary>
    private TransportConnection_SteamNetworkingSockets FindConnection(HSteamNetConnection steamConnectionHandle)
    {
        foreach (TransportConnection_SteamNetworkingSockets transportConnection in transportConnections)
        {
            if (transportConnection.steamConnectionHandle == steamConnectionHandle)
            {
                return transportConnection;
            }
        }
        return null;
    }

    private void OnUpdate()
    {
        LogDebugOutput();
    }

    private void OnLogMemoryUsage(List<string> results)
    {
        results.Add($"Steam networking sockets transport connections: {transportConnections.Count}");
    }

    private void OnSteamNetworkingFakeIpResultCallback(SteamNetworkingFakeIPResult_t callback)
    {
        if (callback.m_eResult == EResult.k_EResultOK)
        {
            CommandWindow.Log("//////////////////////////////////////////////////////");
            Local localization = SDG.Unturned.Provider.localization;
            string arg = new IPv4Address(callback.m_unIP).ToString();
            string arg2 = $"{arg}:{callback.m_unPorts[0]}";
            CommandWindow.Log(localization.format("FakeIPHeader", arg2));
            CommandWindow.Log(localization.format("FakeIPDetails"));
            CommandWindow.Log(localization.format("FakeIPCopy", "CopyFakeIP"));
            CommandWindow.Log("//////////////////////////////////////////////////////");
        }
        else
        {
            CommandWindow.LogError($"Fatal FakeIP result: {callback.m_eResult}");
            SDG.Unturned.Provider.QuitGame($"fatal fake IP result ({callback.m_eResult})");
        }
    }

    private void OnSteamNetConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t callback)
    {
        switch (callback.m_info.m_eState)
        {
        case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_None:
            break;
        case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
            HandleState_Connecting(ref callback);
            break;
        case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_FindingRoute:
            break;
        case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
            HandleState_Connected(ref callback);
            break;
        case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
            HandleState_ClosedByPeer(ref callback);
            break;
        case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
            HandleState_ProblemDetectedLocally(ref callback);
            break;
        }
    }

    private void HandleState_Connecting(ref SteamNetConnectionStatusChangedCallback_t callback)
    {
        if (SDG.Unturned.Provider.didServerShutdownTimerReachZero)
        {
            if (!SteamGameServerNetworkingSockets.CloseConnection(callback.m_hConn, 1002, null, bEnableLinger: false))
            {
                Log("Server failed to close connecting connection while shutdown from {0} (End Reason: {1})", IdentityToString(ref callback), 1002);
            }
        }
        else if (SDG.Unturned.Provider.hasRoomForNewConnection)
        {
            bool num = SteamGameServerNetworkingSockets.SetConnectionPollGroup(callback.m_hConn, pollGroup);
            EResult eResult = SteamGameServerNetworkingSockets.AcceptConnection(callback.m_hConn);
            if (!num || eResult != EResult.k_EResultOK)
            {
                SteamGameServerNetworkingSockets.CloseConnection(callback.m_hConn, 0, null, bEnableLinger: false);
            }
        }
        else if (!SteamGameServerNetworkingSockets.CloseConnection(callback.m_hConn, 1001, null, bEnableLinger: false))
        {
            Log("Server failed to close connecting connection from {0} (End Reason: {1})", IdentityToString(ref callback), 1001);
        }
    }

    private void HandleState_Connected(ref SteamNetConnectionStatusChangedCallback_t callback)
    {
        TransportConnection_SteamNetworkingSockets item = new TransportConnection_SteamNetworkingSockets(this, ref callback);
        transportConnections.Add(item);
    }

    /// <summary>
    /// Must close the handle to free up resources.
    /// </summary>
    private void HandleState_ClosedByPeer(ref SteamNetConnectionStatusChangedCallback_t callback)
    {
        TransportConnection_SteamNetworkingSockets transportConnection_SteamNetworkingSockets = FindConnection(callback.m_hConn);
        if (transportConnection_SteamNetworkingSockets != null)
        {
            try
            {
                string debugString = $"ClosedByPeer Reason: {callback.m_info.m_eEndReason} Message: \"{callback.m_info.m_szEndDebug}\"";
                bool isError = callback.m_info.m_eEndReason != 1000;
                connectionFailureCallback(transportConnection_SteamNetworkingSockets, debugString, isError);
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, "SteamNetworkingSockets caught exception during closed by peer failure callback:");
            }
            transportConnection_SteamNetworkingSockets.CloseConnection();
        }
        else
        {
            SteamGameServerNetworkingSockets.CloseConnection(callback.m_hConn, 0, null, bEnableLinger: false);
        }
    }

    /// <summary>
    /// Must close the handle to free up resources.
    /// </summary>
    private void HandleState_ProblemDetectedLocally(ref SteamNetConnectionStatusChangedCallback_t callback)
    {
        TransportConnection_SteamNetworkingSockets transportConnection_SteamNetworkingSockets = FindConnection(callback.m_hConn);
        if (transportConnection_SteamNetworkingSockets != null)
        {
            try
            {
                string debugString = $"ProblemDetectedLocally Reason: {callback.m_info.m_eEndReason} Message: \"{callback.m_info.m_szEndDebug}\"";
                connectionFailureCallback(transportConnection_SteamNetworkingSockets, debugString, isError: true);
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, "SteamNetworkingSockets caught exception during problem detected locally failure callback:");
            }
            transportConnection_SteamNetworkingSockets.CloseConnection();
        }
        else
        {
            SteamGameServerNetworkingSockets.CloseConnection(callback.m_hConn, 0, null, bEnableLinger: false);
        }
    }

    private void OnSteamNetAuthenticationStatusChanged(SteamNetAuthenticationStatus_t callback)
    {
        if (string.IsNullOrEmpty(callback.m_debugMsg))
        {
            Log("Readiness to participate in authenticated communications changed to {0}", callback.m_eAvail);
        }
        else
        {
            Log("Readiness to participate in authenticated communications changed to {0} \"{1}\"", callback.m_eAvail, callback.m_debugMsg);
        }
    }
}
