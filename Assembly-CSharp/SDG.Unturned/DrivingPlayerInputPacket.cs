using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

public class DrivingPlayerInputPacket : PlayerInputPacket
{
    public Vector3 position;

    public Quaternion rotation;

    public float speed;

    public float forwardVelocity;

    public float steeringInput;

    public float velocityInput;

    internal InteractableVehicle vehicle;

    public override void read(SteamChannel channel, NetPakReader reader)
    {
        base.read(channel, reader);
        reader.ReadClampedVector3(out position, 13, 8);
        reader.ReadQuaternion(out rotation, 11);
        reader.ReadUnsignedClampedFloat(8, 2, out speed);
        reader.ReadClampedFloat(9, 2, out forwardVelocity);
        reader.ReadSignedNormalizedFloat(2, out steeringInput);
        reader.ReadClampedFloat(9, 2, out velocityInput);
        if (!(vehicle != null) || vehicle.asset == null)
        {
            return;
        }
        if (vehicle.asset.replicatedWheelIndices != null)
        {
            int[] replicatedWheelIndices = vehicle.asset.replicatedWheelIndices;
            foreach (int num in replicatedWheelIndices)
            {
                Wheel wheelAtIndex = vehicle.GetWheelAtIndex(num);
                if (wheelAtIndex == null)
                {
                    UnturnedLog.error($"Missing wheel for replicated index: {num}");
                    reader.ReadUnsignedNormalizedFloat(4, out var _);
                    reader.ReadPhysicsMaterialNetId(out var _);
                    continue;
                }
                if (reader.ReadUnsignedNormalizedFloat(4, out var value3))
                {
                    wheelAtIndex.replicatedSuspensionState = value3;
                }
                reader.ReadPhysicsMaterialNetId(out wheelAtIndex.replicatedGroundMaterial);
            }
        }
        if (vehicle.asset.UsesEngineRpmAndGears)
        {
            reader.ReadBits(3, out var value4);
            int value5 = (int)(value4 - 1);
            value5 = Mathf.Clamp(value5, -1, vehicle.asset.forwardGearRatios.Length);
            vehicle.GearNumber = value5;
            reader.ReadUnsignedNormalizedFloat(7, out var value6);
            vehicle.ReplicatedEngineRpm = Mathf.Lerp(vehicle.asset.EngineIdleRpm, vehicle.asset.EngineMaxRpm, value6);
        }
    }

    public override void write(NetPakWriter writer)
    {
        base.write(writer);
        writer.WriteClampedVector3(position, 13, 8);
        writer.WriteQuaternion(rotation, 11);
        writer.WriteUnsignedClampedFloat(speed, 8, 2);
        writer.WriteClampedFloat(forwardVelocity, 9, 2);
        writer.WriteSignedNormalizedFloat(steeringInput, 2);
        writer.WriteClampedFloat(velocityInput, 9, 2);
        if (!(vehicle != null) || vehicle.asset == null)
        {
            return;
        }
        if (vehicle.asset.replicatedWheelIndices != null)
        {
            int[] replicatedWheelIndices = vehicle.asset.replicatedWheelIndices;
            foreach (int num in replicatedWheelIndices)
            {
                Wheel wheelAtIndex = vehicle.GetWheelAtIndex(num);
                if (wheelAtIndex == null)
                {
                    UnturnedLog.error($"Missing wheel for replicated index: {num}");
                    writer.WriteUnsignedNormalizedFloat(0f, 4);
                    writer.WritePhysicsMaterialNetId(PhysicsMaterialNetId.NULL);
                }
                else
                {
                    writer.WriteUnsignedNormalizedFloat(wheelAtIndex.replicatedSuspensionState, 4);
                    writer.WritePhysicsMaterialNetId(wheelAtIndex.replicatedGroundMaterial);
                }
            }
        }
        if (vehicle.asset.UsesEngineRpmAndGears)
        {
            uint value = (uint)(vehicle.GearNumber + 1);
            writer.WriteBits(value, 3);
            float value2 = Mathf.InverseLerp(vehicle.asset.EngineIdleRpm, vehicle.asset.EngineMaxRpm, vehicle.ReplicatedEngineRpm);
            writer.WriteUnsignedNormalizedFloat(value2, 7);
        }
    }

    public DrivingPlayerInputPacket(InteractableVehicle vehicle)
    {
        this.vehicle = vehicle;
    }
}
