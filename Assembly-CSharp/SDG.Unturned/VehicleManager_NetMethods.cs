using SDG.NetPak;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(VehicleManager))]
public static class VehicleManager_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveVehicleLockState", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVehicleLockState_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        reader.ReadSteamID(out CSteamID value2);
        reader.ReadSteamID(out CSteamID value3);
        reader.ReadBit(out var value4);
        VehicleManager.ReceiveVehicleLockState(value, value2, value3, value4);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleLockState", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVehicleLockState_Write(NetPakWriter writer, uint instanceID, CSteamID owner, CSteamID group, bool locked)
    {
        writer.WriteUInt32(instanceID);
        writer.WriteSteamID(owner);
        writer.WriteSteamID(group);
        writer.WriteBit(locked);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleSkin", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVehicleSkin_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        reader.ReadUInt16(out var value2);
        reader.ReadUInt16(out var value3);
        VehicleManager.ReceiveVehicleSkin(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleSkin", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVehicleSkin_Write(NetPakWriter writer, uint instanceID, ushort skinID, ushort mythicID)
    {
        writer.WriteUInt32(instanceID);
        writer.WriteUInt16(skinID);
        writer.WriteUInt16(mythicID);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleSirens", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVehicleSirens_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        reader.ReadBit(out var value2);
        VehicleManager.ReceiveVehicleSirens(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleSirens", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVehicleSirens_Write(NetPakWriter writer, uint instanceID, bool on)
    {
        writer.WriteUInt32(instanceID);
        writer.WriteBit(on);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleBlimp", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVehicleBlimp_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        reader.ReadBit(out var value2);
        VehicleManager.ReceiveVehicleBlimp(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleBlimp", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVehicleBlimp_Write(NetPakWriter writer, uint instanceID, bool on)
    {
        writer.WriteUInt32(instanceID);
        writer.WriteBit(on);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleHeadlights", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVehicleHeadlights_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        reader.ReadBit(out var value2);
        VehicleManager.ReceiveVehicleHeadlights(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleHeadlights", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVehicleHeadlights_Write(NetPakWriter writer, uint instanceID, bool on)
    {
        writer.WriteUInt32(instanceID);
        writer.WriteBit(on);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleHorn", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVehicleHorn_Read(in ClientInvocationContext context)
    {
        context.reader.ReadUInt32(out var value);
        VehicleManager.ReceiveVehicleHorn(value);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleHorn", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVehicleHorn_Write(NetPakWriter writer, uint instanceID)
    {
        writer.WriteUInt32(instanceID);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleFuel", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVehicleFuel_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        reader.ReadUInt16(out var value2);
        VehicleManager.ReceiveVehicleFuel(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleFuel", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVehicleFuel_Write(NetPakWriter writer, uint instanceID, ushort newFuel)
    {
        writer.WriteUInt32(instanceID);
        writer.WriteUInt16(newFuel);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleBatteryCharge", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVehicleBatteryCharge_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        reader.ReadUInt16(out var value2);
        VehicleManager.ReceiveVehicleBatteryCharge(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleBatteryCharge", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVehicleBatteryCharge_Write(NetPakWriter writer, uint instanceID, ushort newBatteryCharge)
    {
        writer.WriteUInt32(instanceID);
        writer.WriteUInt16(newBatteryCharge);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleTireAliveMask", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVehicleTireAliveMask_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        reader.ReadUInt8(out var value2);
        VehicleManager.ReceiveVehicleTireAliveMask(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleTireAliveMask", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVehicleTireAliveMask_Write(NetPakWriter writer, uint instanceID, byte newTireAliveMask)
    {
        writer.WriteUInt32(instanceID);
        writer.WriteUInt8(newTireAliveMask);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleExploded", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVehicleExploded_Read(in ClientInvocationContext context)
    {
        context.reader.ReadUInt32(out var value);
        VehicleManager.ReceiveVehicleExploded(value);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleExploded", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVehicleExploded_Write(NetPakWriter writer, uint instanceID)
    {
        writer.WriteUInt32(instanceID);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleHealth", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVehicleHealth_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        reader.ReadUInt16(out var value2);
        VehicleManager.ReceiveVehicleHealth(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleHealth", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVehicleHealth_Write(NetPakWriter writer, uint instanceID, ushort newHealth)
    {
        writer.WriteUInt32(instanceID);
        writer.WriteUInt16(newHealth);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleRecov", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVehicleRecov_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        reader.ReadClampedVector3(out var value2);
        reader.ReadInt32(out var value3);
        VehicleManager.ReceiveVehicleRecov(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveVehicleRecov", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVehicleRecov_Write(NetPakWriter writer, uint instanceID, Vector3 newPosition, int newRecov)
    {
        writer.WriteUInt32(instanceID);
        writer.WriteClampedVector3(newPosition);
        writer.WriteInt32(newRecov);
    }

    [NetInvokableGeneratedMethod("ReceiveDestroySingleVehicle", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveDestroySingleVehicle_Read(in ClientInvocationContext context)
    {
        context.reader.ReadUInt32(out var value);
        VehicleManager.ReceiveDestroySingleVehicle(value);
    }

    [NetInvokableGeneratedMethod("ReceiveDestroySingleVehicle", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveDestroySingleVehicle_Write(NetPakWriter writer, uint instanceID)
    {
        writer.WriteUInt32(instanceID);
    }

    [NetInvokableGeneratedMethod("ReceiveDestroyAllVehicles", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveDestroyAllVehicles_Read(in ClientInvocationContext context)
    {
        VehicleManager.ReceiveDestroyAllVehicles();
    }

    [NetInvokableGeneratedMethod("ReceiveDestroyAllVehicles", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveDestroyAllVehicles_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceiveEnterVehicle", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveEnterVehicle_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadSteamID(out CSteamID value3);
        VehicleManager.ReceiveEnterVehicle(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveEnterVehicle", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveEnterVehicle_Write(NetPakWriter writer, uint instanceID, byte seat, CSteamID player)
    {
        writer.WriteUInt32(instanceID);
        writer.WriteUInt8(seat);
        writer.WriteSteamID(player);
    }

    [NetInvokableGeneratedMethod("ReceiveExitVehicle", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveExitVehicle_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadClampedVector3(out var value3);
        reader.ReadUInt8(out var value4);
        reader.ReadBit(out var value5);
        VehicleManager.ReceiveExitVehicle(value, value2, value3, value4, value5);
    }

    [NetInvokableGeneratedMethod("ReceiveExitVehicle", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveExitVehicle_Write(NetPakWriter writer, uint instanceID, byte seat, Vector3 point, byte angle, bool forceUpdate)
    {
        writer.WriteUInt32(instanceID);
        writer.WriteUInt8(seat);
        writer.WriteClampedVector3(point);
        writer.WriteUInt8(angle);
        writer.WriteBit(forceUpdate);
    }

    [NetInvokableGeneratedMethod("ReceiveSwapVehicleSeats", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSwapVehicleSeats_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadUInt8(out var value3);
        VehicleManager.ReceiveSwapVehicleSeats(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveSwapVehicleSeats", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSwapVehicleSeats_Write(NetPakWriter writer, uint instanceID, byte fromSeat, byte toSeat)
    {
        writer.WriteUInt32(instanceID);
        writer.WriteUInt8(fromSeat);
        writer.WriteUInt8(toSeat);
    }

    [NetInvokableGeneratedMethod("ReceiveToggleVehicleHeadlights", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveToggleVehicleHeadlights_Read(in ServerInvocationContext context)
    {
        context.reader.ReadBit(out var value);
        VehicleManager.ReceiveToggleVehicleHeadlights(in context, value);
    }

    [NetInvokableGeneratedMethod("ReceiveToggleVehicleHeadlights", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveToggleVehicleHeadlights_Write(NetPakWriter writer, bool wantsHeadlightsOn)
    {
        writer.WriteBit(wantsHeadlightsOn);
    }

    [NetInvokableGeneratedMethod("ReceiveUseVehicleBonus", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUseVehicleBonus_Read(in ServerInvocationContext context)
    {
        context.reader.ReadUInt8(out var value);
        VehicleManager.ReceiveUseVehicleBonus(in context, value);
    }

    [NetInvokableGeneratedMethod("ReceiveUseVehicleBonus", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUseVehicleBonus_Write(NetPakWriter writer, byte bonusType)
    {
        writer.WriteUInt8(bonusType);
    }

    [NetInvokableGeneratedMethod("ReceiveEnterVehicleRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveEnterVehicleRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        reader.ReadUInt8(out var value2);
        byte[] array = new byte[value2];
        reader.ReadBytes(array);
        reader.ReadUInt8(out var value3);
        byte[] array2 = new byte[value3];
        reader.ReadBytes(array2);
        reader.ReadUInt8(out var value4);
        VehicleManager.ReceiveEnterVehicleRequest(in context, value, array, array2, value4);
    }

    [NetInvokableGeneratedMethod("ReceiveEnterVehicleRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveEnterVehicleRequest_Write(NetPakWriter writer, uint instanceID, byte[] hash, byte[] physicsProfileHash, byte engine)
    {
        writer.WriteUInt32(instanceID);
        byte b = (byte)hash.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(hash, b);
        byte b2 = (byte)physicsProfileHash.Length;
        writer.WriteUInt8(b2);
        writer.WriteBytes(physicsProfileHash, b2);
        writer.WriteUInt8(engine);
    }

    [NetInvokableGeneratedMethod("ReceiveExitVehicleRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveExitVehicleRequest_Read(in ServerInvocationContext context)
    {
        context.reader.ReadClampedVector3(out var value);
        VehicleManager.ReceiveExitVehicleRequest(in context, value);
    }

    [NetInvokableGeneratedMethod("ReceiveExitVehicleRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveExitVehicleRequest_Write(NetPakWriter writer, Vector3 velocity)
    {
        writer.WriteClampedVector3(velocity);
    }

    [NetInvokableGeneratedMethod("ReceiveSwapVehicleRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSwapVehicleRequest_Read(in ServerInvocationContext context)
    {
        context.reader.ReadUInt8(out var value);
        VehicleManager.ReceiveSwapVehicleRequest(in context, value);
    }

    [NetInvokableGeneratedMethod("ReceiveSwapVehicleRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSwapVehicleRequest_Write(NetPakWriter writer, byte toSeat)
    {
        writer.WriteUInt8(toSeat);
    }
}
