using System;
using SDG.NetPak;

namespace SDG.Unturned;

internal static class ClientMessageHandler_BattlEye
{
    internal unsafe static void ReadMessage(NetPakReader reader)
    {
        if (!(Provider.battlEyeClientHandle != IntPtr.Zero) || Provider.battlEyeClientRunData == null || Provider.battlEyeClientRunData.pfnReceivedPacket == null)
        {
            return;
        }
        reader.ReadBits(Provider.battlEyeBufferSize.bitCount, out var value);
        if (value != 0 && reader.ReadBytesPtr((int)value, out var source, out var bufferOffset))
        {
            fixed (byte* ptr = source)
            {
                IntPtr pvPacket = new IntPtr(ptr + bufferOffset);
                Provider.battlEyeClientRunData.pfnReceivedPacket(pvPacket, (int)value);
            }
        }
    }
}
