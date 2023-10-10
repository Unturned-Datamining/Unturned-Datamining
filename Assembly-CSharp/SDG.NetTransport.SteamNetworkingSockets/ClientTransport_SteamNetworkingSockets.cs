using System;
using System.Runtime.InteropServices;
using SDG.Framework.Utilities;
using SDG.Unturned;
using Steamworks;

namespace SDG.NetTransport.SteamNetworkingSockets;

public class ClientTransport_SteamNetworkingSockets : TransportBase_SteamNetworkingSockets, IClientTransport
{
    private Callback<SteamNetConnectionStatusChangedCallback_t> steamNetConnectionStatusChanged;

    private Callback<SteamNetAuthenticationStatus_t> steamNetAuthenticationStatusChanged;

    private ClientTransportReady connectedCallback;

    private ClientTransportFailure failureCallback;

    private HSteamNetConnection connection = HSteamNetConnection.Invalid;

    private bool isWaitingForAuthAvailability;

    private bool isConnected;

    private bool didCloseConnection;

    private bool didSetupDebugOutput;

    private IntPtr[] messageAddresses = new IntPtr[1];

    public void Initialize(ClientTransportReady callback, ClientTransportFailure failureCallback)
    {
        connectedCallback = callback;
        this.failureCallback = failureCallback;
        steamNetConnectionStatusChanged = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnSteamNetConnectionStatusChanged);
        steamNetAuthenticationStatusChanged = Callback<SteamNetAuthenticationStatus_t>.Create(OnSteamNetAuthenticationStatusChanged);
        ESteamNetworkingSocketsDebugOutputType eSteamNetworkingSocketsDebugOutputType = SelectDebugOutputDetailLevel();
        if (eSteamNetworkingSocketsDebugOutputType != 0)
        {
            didSetupDebugOutput = true;
            Log("Client set SNS debug output detail level to {0}", eSteamNetworkingSocketsDebugOutputType);
            SteamNetworkingUtils.SetDebugOutputFunction(eSteamNetworkingSocketsDebugOutputType, GetDebugOutputFunction());
        }
        TimeUtility.updated += OnUpdate;
        if ((bool)TransportBase_SteamNetworkingSockets.clAllowWithoutAuth)
        {
            isWaitingForAuthAvailability = false;
            Log("Client bypassing test for Steam Networking availability");
            Connect();
            return;
        }
        isWaitingForAuthAvailability = true;
        ESteamNetworkingAvailability eSteamNetworkingAvailability = Steamworks.SteamNetworkingSockets.InitAuthentication();
        if (eSteamNetworkingAvailability != ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Current)
        {
            Log("Client testing for Steam Networking availability ({0})", eSteamNetworkingAvailability);
        }
        HandleAuth(eSteamNetworkingAvailability);
    }

    public void TearDown()
    {
        steamNetConnectionStatusChanged.Dispose();
        steamNetAuthenticationStatusChanged.Dispose();
        if (!didCloseConnection && connection != HSteamNetConnection.Invalid)
        {
            didCloseConnection = true;
            bool flag = Steamworks.SteamNetworkingSockets.CloseConnection(connection, 0, null, bEnableLinger: true);
            Log("Client disconnect from {0} result: {1}", connection, flag);
        }
        TimeUtility.updated -= OnUpdate;
        if (didSetupDebugOutput)
        {
            didSetupDebugOutput = false;
            SteamNetworkingUtils.SetDebugOutputFunction(ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_None, null);
        }
    }

    public unsafe void Send(byte[] buffer, long size, ENetReliability reliability)
    {
        if (isConnected && !didCloseConnection)
        {
            int nSendFlags = ReliabilityToSendFlags(reliability);
            fixed (byte* value = buffer)
            {
                Steamworks.SteamNetworkingSockets.SendMessageToConnection(pData: new IntPtr(value), hConn: connection, cbData: (uint)size, nSendFlags: nSendFlags, pOutMessageNumber: out var _);
            }
        }
    }

    public bool Receive(byte[] buffer, out long size)
    {
        size = 0L;
        if (!isConnected || didCloseConnection)
        {
            return false;
        }
        IntPtr intPtr;
        SteamNetworkingMessage_t steamNetworkingMessage_t;
        while (true)
        {
            if (Steamworks.SteamNetworkingSockets.ReceiveMessagesOnConnection(connection, messageAddresses, messageAddresses.Length) < 1)
            {
                return false;
            }
            intPtr = messageAddresses[0];
            steamNetworkingMessage_t = Marshal.PtrToStructure<SteamNetworkingMessage_t>(intPtr);
            if (!(steamNetworkingMessage_t.m_pData == IntPtr.Zero) && steamNetworkingMessage_t.m_cbSize >= 1)
            {
                break;
            }
            SteamNetworkingMessage_t.Release(intPtr);
        }
        size = steamNetworkingMessage_t.m_cbSize;
        if (size > buffer.Length)
        {
            size = buffer.Length;
        }
        Marshal.Copy(steamNetworkingMessage_t.m_pData, buffer, 0, (int)size);
        SteamNetworkingMessage_t.Release(intPtr);
        return true;
    }

    private void OnUpdate()
    {
        LogDebugOutput();
    }

    private void Connect()
    {
        SteamNetworkingConfigValue_t[] array = BuildDefaultConfig().ToArray();
        SteamNetworkingIPAddr address = default(SteamNetworkingIPAddr);
        address.SetIPv4(SDG.Unturned.Provider.currentServerInfo.ip, SDG.Unturned.Provider.currentServerInfo.connectionPort);
        connection = Steamworks.SteamNetworkingSockets.ConnectByIPAddress(ref address, array.Length, array);
        Log("Client connecting to {0}", AddressToString(address));
    }

    private void OnSteamNetConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t callback)
    {
        if (!(callback.m_hConn != connection))
        {
            switch (callback.m_info.m_eState)
            {
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_None:
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
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
    }

    private void HandleState_Connected(ref SteamNetConnectionStatusChangedCallback_t callback)
    {
        if (connectedCallback != null)
        {
            isConnected = true;
            Log("Client connection with {0} ready", connection);
            connectedCallback();
            connectedCallback = null;
        }
    }

    private string GetMessageForEndReason(int endReasonCode)
    {
        ESteamNetConnectionEnd eSteamNetConnectionEnd;
        try
        {
            eSteamNetConnectionEnd = (ESteamNetConnectionEnd)endReasonCode;
        }
        catch
        {
            eSteamNetConnectionEnd = ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Invalid;
        }
        if (endReasonCode >= 1000 && endReasonCode <= 1999)
        {
            switch (endReasonCode)
            {
            case 1001:
                return GetMessageText("SteamNetworkingSockets_EndReason_App_1001");
            case 1002:
                return GetMessageText("SteamNetworkingSockets_EndReason_App_1002");
            default:
                if (eSteamNetConnectionEnd == ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_App_Min)
                {
                    return null;
                }
                return GetMessageText("SteamNetworkingSockets_EndReason_App_Unknown", endReasonCode);
            }
        }
        if (endReasonCode >= 2000 && endReasonCode <= 2999)
        {
            if (eSteamNetConnectionEnd == ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_AppException_Min)
            {
                return GetMessageText("SteamNetworkingSockets_EndReason_AppException_Generic");
            }
            return GetMessageText("SteamNetworkingSockets_EndReason_AppException_Unknown", endReasonCode);
        }
        if (endReasonCode >= 3000 && endReasonCode <= 3999)
        {
            switch (eSteamNetConnectionEnd)
            {
            case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Local_OfflineMode:
                return GetMessageText("SteamNetworkingSockets_EndReason_Local_OfflineMode");
            case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Local_ManyRelayConnectivity:
            case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Local_HostedServerPrimaryRelay:
                return GetMessageText("SteamNetworkingSockets_EndReason_Local_RelayConnectivity");
            case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Local_NetworkConfig:
                return GetMessageText("SteamNetworkingSockets_EndReason_Local_NetworkConfig");
            case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Local_Rights:
                return GetMessageText("SteamNetworkingSockets_EndReason_Local_Rights");
            default:
                return GetMessageText("SteamNetworkingSockets_EndReason_Local_Unknown", endReasonCode);
            }
        }
        if (endReasonCode >= 4000 && endReasonCode <= 4999)
        {
            return eSteamNetConnectionEnd switch
            {
                ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Remote_BadCrypt => GetMessageText("SteamNetworkingSockets_EndReason_Remote_BadCrypt"), 
                ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Remote_BadCert => GetMessageText("SteamNetworkingSockets_EndReason_Remote_BadCert"), 
                ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Remote_Timeout => GetMessageText("SteamNetworkingSockets_EndReason_Remote_Timeout"), 
                ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Remote_BadProtocolVersion => GetMessageText("SteamNetworkingSockets_EndReason_Remote_BadProtocolVersion"), 
                _ => GetMessageText("SteamNetworkingSockets_EndReason_Remote_Unknown", endReasonCode), 
            };
        }
        if (endReasonCode >= 5000 && endReasonCode <= 5999)
        {
            return eSteamNetConnectionEnd switch
            {
                ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Misc_InternalError => GetMessageText("SteamNetworkingSockets_EndReason_Misc_InternalError"), 
                ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Misc_Timeout => GetMessageText("SteamNetworkingSockets_EndReason_Misc_Timeout"), 
                ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Misc_SteamConnectivity => GetMessageText("SteamNetworkingSockets_EndReason_Misc_SteamConnectivity"), 
                _ => GetMessageText("SteamNetworkingSockets_EndReason_Misc_Unknown", endReasonCode), 
            };
        }
        return GetMessageText("SteamNetworkingSockets_EndReason_Unknown", endReasonCode);
    }

    private void InvokeFailureCallback(string message)
    {
        if (failureCallback != null)
        {
            ClientTransportFailure clientTransportFailure = failureCallback;
            failureCallback = null;
            clientTransportFailure(message);
        }
    }

    private void InvokeFailureCallback(int endReasonCode)
    {
        if (failureCallback != null)
        {
            string messageForEndReason = GetMessageForEndReason(endReasonCode);
            if (!string.IsNullOrEmpty(messageForEndReason))
            {
                InvokeFailureCallback(messageForEndReason);
            }
        }
    }

    private void HandleState_ClosedByPeer(ref SteamNetConnectionStatusChangedCallback_t callback)
    {
        Log("Client connection closed by peer ({0}) \"{1}\"", callback.m_info.m_eEndReason, callback.m_info.m_szEndDebug);
        didCloseConnection = true;
        if (!Steamworks.SteamNetworkingSockets.CloseConnection(callback.m_hConn, 0, null, bEnableLinger: false))
        {
            Log("Client failed to release connection closed by peer");
        }
        InvokeFailureCallback(callback.m_info.m_eEndReason);
    }

    private void HandleState_ProblemDetectedLocally(ref SteamNetConnectionStatusChangedCallback_t callback)
    {
        Log("Client connection problem detected locally ({0}) \"{1}\"", callback.m_info.m_eEndReason, callback.m_info.m_szEndDebug);
        didCloseConnection = true;
        if (!Steamworks.SteamNetworkingSockets.CloseConnection(callback.m_hConn, 0, null, bEnableLinger: false))
        {
            Log("Client failed to release connection after problem detected locally");
        }
        InvokeFailureCallback(callback.m_info.m_eEndReason);
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
        if (isWaitingForAuthAvailability)
        {
            HandleAuth(callback.m_eAvail);
        }
    }

    private void HandleAuth(ESteamNetworkingAvailability authAvailability)
    {
        switch (authAvailability)
        {
        case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_CannotTry:
            HandleAuth_CannotTry();
            break;
        case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Failed:
            HandleAuth_Failed();
            break;
        case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Previously:
            HandleAuth_Previously();
            break;
        case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Current:
            HandleAuth_Current();
            break;
        }
    }

    private void HandleAuth_CannotTry()
    {
        isWaitingForAuthAvailability = false;
        InvokeFailureCallback(GetMessageText("SteamNetworkingSockets_Unavailable_CannotTry"));
    }

    private void HandleAuth_Failed()
    {
        isWaitingForAuthAvailability = false;
        InvokeFailureCallback(GetMessageText("SteamNetworkingSockets_Unavailable_Failed"));
    }

    private void HandleAuth_Previously()
    {
        isWaitingForAuthAvailability = false;
        InvokeFailureCallback(GetMessageText("SteamNetworkingSockets_Unavailable_Previously"));
    }

    private void HandleAuth_Current()
    {
        Log("Client Steam Networking available");
        isWaitingForAuthAvailability = false;
        Connect();
    }
}
