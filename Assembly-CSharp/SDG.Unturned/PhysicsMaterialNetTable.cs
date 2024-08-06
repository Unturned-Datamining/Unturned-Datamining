using System;
using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned;

/// <summary>
/// String table specifically for Unity physics material names.
/// Implemented so that tires can more efficiently replicate which ground material they are touching.
/// </summary>
public static class PhysicsMaterialNetTable
{
    private static readonly ClientStaticMethod SendMappings = ClientStaticMethod.Get(ReceiveMappings);

    /// <summary>
    /// Number of bits needed to replicate PhysicsMaterialNetId.
    /// </summary>
    internal static int idBitCount;

    private static Dictionary<string, uint> nameToId = new Dictionary<string, uint>();

    private static Dictionary<uint, string> idToName = new Dictionary<uint, string>();

    /// <summary>
    /// Get an ID that can be used to reference a physics material name over the network. If given material name
    /// isn't supported (e.g., not registered in a PhysicsMaterialAsset or over max material limit) returns
    /// <see cref="F:SDG.Unturned.PhysicsMaterialNetId.NULL" /> instead.
    /// </summary>
    public static PhysicsMaterialNetId GetNetId(string materialName)
    {
        if (string.IsNullOrEmpty(materialName))
        {
            return PhysicsMaterialNetId.NULL;
        }
        if (nameToId.TryGetValue(materialName, out var value))
        {
            return new PhysicsMaterialNetId(value);
        }
        return PhysicsMaterialNetId.NULL;
    }

    /// <summary>
    /// Get name of a physics material from network ID. Returns null if ID is null, e.g., if the sent name wasn't
    /// registered or was over the max material limit.
    /// </summary>
    public static string GetMaterialName(PhysicsMaterialNetId netId)
    {
        if (netId.id != 0 && idToName.TryGetValue(netId.id, out var value))
        {
            return value;
        }
        return null;
    }

    /// <summary>
    /// Called when resetting network state.
    /// </summary>
    internal static void Clear()
    {
        nameToId.Clear();
        idToName.Clear();
    }

    /// <summary>
    /// Called on server and singleplayer before loading level.
    /// </summary>
    internal static void ServerPopulateTable()
    {
        uint num = 1u;
        foreach (KeyValuePair<Guid, PhysicsMaterialAsset> asset in PhysicMaterialCustomData.GetAssets())
        {
            PhysicsMaterialAsset value = asset.Value;
            if (value.physicMaterialNames == null || value.physicMaterialNames.Length < 1)
            {
                continue;
            }
            uint num2 = num;
            num++;
            bool flag = false;
            string[] physicMaterialNames = value.physicMaterialNames;
            foreach (string text in physicMaterialNames)
            {
                if (nameToId.ContainsKey(text))
                {
                    UnturnedLog.warn("Multiple physics material assets contain Unity name \"" + text + "\"");
                    continue;
                }
                nameToId[text] = num2;
                if (!flag)
                {
                    idToName[num2] = text;
                    flag = true;
                }
            }
        }
        idBitCount = NetPakConst.CountBits(num);
        UnturnedLog.info($"Server registered {nameToId.Count} Unity physics material names with {num - 1} unique IDs ({idBitCount} bits)");
    }

    internal static void Send(ITransportConnection transportConnection)
    {
        SendMappings.Invoke(ENetReliability.Reliable, transportConnection, delegate(NetPakWriter writer)
        {
            writer.WriteUInt8((byte)nameToId.Count);
            writer.WriteUInt8((byte)idBitCount);
            foreach (KeyValuePair<string, uint> item in nameToId)
            {
                string key = item.Key;
                uint value = item.Value;
                writer.WriteString(key, 6);
                writer.WriteBits(value, idBitCount);
            }
        });
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveMappings(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        idBitCount = value2;
        for (int i = 0; i < value; i++)
        {
            reader.ReadString(out var value3, 6);
            reader.ReadBits(idBitCount, out var value4);
            nameToId[value3] = value4;
            idToName[value4] = value3;
        }
        UnturnedLog.info($"Client received {nameToId.Count} Unity physics material names ({idBitCount} bits)");
    }
}
