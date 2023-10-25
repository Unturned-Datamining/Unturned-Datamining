using System;
using System.Diagnostics;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;

namespace SDG.Unturned;

/// <summary>
/// Optional parameter for error logging and responding to the invoker.
/// </summary>
public readonly struct ServerInvocationContext
{
    public enum EOrigin
    {
        Remote,
        Loopback,
        Obsolete
    }

    public readonly EOrigin origin;

    public readonly NetPakReader reader;

    private readonly SteamPlayer callingPlayer;

    private readonly ServerMethodInfo serverMethodInfo;

    internal bool IsOwnerOf(SteamChannel legacyComponent)
    {
        if (legacyComponent.owner != null)
        {
            return legacyComponent.owner == callingPlayer;
        }
        return false;
    }

    public Player GetPlayer()
    {
        return callingPlayer?.player;
    }

    public SteamPlayer GetCallingPlayer()
    {
        return callingPlayer;
    }

    public ITransportConnection GetTransportConnection()
    {
        return callingPlayer.transportConnection;
    }

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("DEBUG_NETINVOKABLES")]
    public void ReadParameterFailed(string parameterName)
    {
        CommandWindow.LogWarningFormat("{0} {1}: unable to read {2}", GetTransportConnection(), serverMethodInfo, parameterName);
    }

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("DEBUG_NETINVOKABLES")]
    public void LogWarning(string message)
    {
        CommandWindow.LogWarningFormat("{0} {1}: {2}", GetTransportConnection(), serverMethodInfo, message);
    }

    public void Kick(string reason)
    {
        if (callingPlayer != null)
        {
            Provider.kick(callingPlayer.playerID.steamID, reason);
        }
    }

    [Obsolete("Only exists for plugins manually calling obsolete RPCs with steamID sender parameter. Do not use directly. Will remove.")]
    internal static ServerInvocationContext FromSteamIDForBackwardsCompatibility(CSteamID steamID)
    {
        return new ServerInvocationContext(steamID);
    }

    internal ServerInvocationContext(EOrigin origin, SteamPlayer callingPlayer, NetPakReader reader, ServerMethodInfo serverMethodInfo)
    {
        this.origin = origin;
        this.callingPlayer = callingPlayer;
        this.reader = reader;
        this.serverMethodInfo = serverMethodInfo;
    }

    [Obsolete("Only exists for plugins manually calling obsolete RPCs with steamID sender parameter.")]
    private ServerInvocationContext(CSteamID steamID)
    {
        origin = EOrigin.Obsolete;
        callingPlayer = PlayerTool.getSteamPlayer(steamID);
        reader = null;
        serverMethodInfo = null;
    }
}
