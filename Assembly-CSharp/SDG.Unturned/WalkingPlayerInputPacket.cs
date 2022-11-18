using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

public class WalkingPlayerInputPacket : PlayerInputPacket
{
    public byte analog;

    public Vector3 clientPosition;

    public float yaw;

    public float pitch;

    public override void read(SteamChannel channel, NetPakReader reader)
    {
        base.read(channel, reader);
        reader.ReadUInt8(out analog);
        reader.ReadClampedVector3(out clientPosition);
        reader.ReadFloat(out yaw);
        reader.ReadFloat(out pitch);
    }

    public override void write(NetPakWriter writer)
    {
        base.write(writer);
        writer.WriteUInt8(analog);
        writer.WriteClampedVector3(clientPosition);
        writer.WriteFloat(yaw);
        writer.WriteFloat(pitch);
    }
}
