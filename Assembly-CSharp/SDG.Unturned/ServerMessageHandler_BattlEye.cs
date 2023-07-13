using System;
using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned;

internal static class ServerMessageHandler_BattlEye
{
    internal unsafe static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
    {
        if (Provider.battlEyeServerHandle != IntPtr.Zero && Provider.battlEyeServerRunData != null && Provider.battlEyeServerRunData.pfnReceivedPacket != null)
        {
            SteamPlayer steamPlayer = Provider.findPlayer(transportConnection);
            if (steamPlayer != null)
            {
                reader.ReadBits(Provider.battlEyeBufferSize.bitCount, out var value);
                if (value != 0 && reader.ReadBytesPtr((int)value, out var source, out var bufferOffset))
                {
                    fixed (byte* ptr = source)
                    {
                        IntPtr pvPacket = new IntPtr(ptr + bufferOffset);
                        Provider.battlEyeServerRunData.pfnReceivedPacket(steamPlayer.battlEyeId, pvPacket, (int)value);
                    }
                }
                else
                {
                    UnturnedLog.warn("Received empty BattlEye payload from {0}, so we're refusing them", transportConnection);
                    Provider.refuseGarbageConnection(transportConnection, "sv empty BE payload");
                }
            }
            else if ((bool)NetMessages.shouldLogBadMessages)
            {
                UnturnedLog.info($"Ignoring BattlEye message from {transportConnection} because there is no associated player");
            }
        }
        else if ((bool)NetMessages.shouldLogBadMessages)
        {
            UnturnedLog.info($"Ignoring BattlEye message from {transportConnection} because BattlEye is not running");
        }
    }
}
