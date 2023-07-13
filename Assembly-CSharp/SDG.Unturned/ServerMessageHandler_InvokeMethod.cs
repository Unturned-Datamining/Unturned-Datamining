using System;
using SDG.NetPak;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

internal static class ServerMessageHandler_InvokeMethod
{
    internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
    {
        if (!reader.ReadBits(NetReflection.serverMethodsBitCount, out var value))
        {
            Provider.refuseGarbageConnection(transportConnection, "unable to read method index");
            return;
        }
        if (value >= NetReflection.serverMethodsLength)
        {
            Provider.refuseGarbageConnection(transportConnection, "out of bounds method index");
            return;
        }
        SteamPlayer steamPlayer = Provider.findPlayer(transportConnection);
        if (steamPlayer == null)
        {
            if ((bool)NetMessages.shouldLogBadMessages)
            {
                UnturnedLog.info($"Ignoring InvokeMethod message from {transportConnection} because there is no associated player");
            }
            return;
        }
        ServerMethodInfo serverMethodInfo = NetReflection.serverMethods[value];
        ServerInvocationContext context = new ServerInvocationContext(ServerInvocationContext.EOrigin.Remote, steamPlayer, reader, serverMethodInfo);
        if (serverMethodInfo.rateLimitIndex >= 0)
        {
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            float num = steamPlayer.rpcAllowedTimes[serverMethodInfo.rateLimitIndex];
            if (realtimeSinceStartup < num)
            {
                steamPlayer.rpcHitCount[serverMethodInfo.rateLimitIndex]++;
                int num2 = Mathf.Max(2, Provider.configData.Server.Rate_Limit_Kick_Threshold);
                if (steamPlayer.rpcHitCount[serverMethodInfo.rateLimitIndex] >= num2)
                {
                    context.Kick($"significantly exceeded {serverMethodInfo} rate limit ({num2} times in {serverMethodInfo.customAttribute.ratelimitSeconds} seconds)");
                }
                return;
            }
            steamPlayer.rpcAllowedTimes[serverMethodInfo.rateLimitIndex] = realtimeSinceStartup + serverMethodInfo.customAttribute.ratelimitSeconds;
            steamPlayer.rpcHitCount[serverMethodInfo.rateLimitIndex] = 0;
        }
        try
        {
            steamPlayer.timeLastPacketWasReceivedFromClient = Time.realtimeSinceStartup;
            serverMethodInfo.readMethod(in context);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Exception invoking {0} from client {1}:", serverMethodInfo, transportConnection);
        }
    }
}
