using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

public class WalkingPlayerInputPacket : PlayerInputPacket
{
    public byte analog;

    /// <summary>
    /// Resulting transform.position immediately after movement.simulate was called.
    /// </summary>
    public Vector3 clientPosition;

    public override void read(SteamChannel channel, NetPakReader reader)
    {
        base.read(channel, reader);
        reader.ReadUInt8(out analog);
        reader.ReadClampedVector3(out clientPosition);
    }

    public override void write(NetPakWriter writer)
    {
        base.write(writer);
        writer.WriteUInt8(analog);
        writer.WriteClampedVector3(clientPosition);
    }
}
