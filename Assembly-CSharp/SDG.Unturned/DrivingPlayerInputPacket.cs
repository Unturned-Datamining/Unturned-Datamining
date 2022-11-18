using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

public class DrivingPlayerInputPacket : PlayerInputPacket
{
    public Vector3 position;

    public Quaternion rotation;

    public byte speed;

    public byte physicsSpeed;

    public byte turn;

    public override void read(SteamChannel channel, NetPakReader reader)
    {
        base.read(channel, reader);
        reader.ReadClampedVector3(out position, 13, 8);
        reader.ReadQuaternion(out rotation, 11);
        reader.ReadUInt8(out speed);
        reader.ReadUInt8(out physicsSpeed);
        reader.ReadUInt8(out turn);
    }

    public override void write(NetPakWriter writer)
    {
        base.write(writer);
        writer.WriteClampedVector3(position, 13, 8);
        writer.WriteQuaternion(rotation, 11);
        writer.WriteUInt8(speed);
        writer.WriteUInt8(physicsSpeed);
        writer.WriteUInt8(turn);
    }
}
