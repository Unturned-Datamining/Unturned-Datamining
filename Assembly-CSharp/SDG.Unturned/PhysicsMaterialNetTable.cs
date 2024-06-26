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

    private static Dictionary<string, uint> nameToId = new Dictionary<string, uint>();

    /// <summary>
    /// Index of ID minus one corresponds to name. This approach allows server to send names in ascending order, so
    /// client can infer the ID without sending it.
    /// </summary>
    private static List<string> idMinusOneToName = new List<string>();

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
        if (netId.id != 0)
        {
            int num = (int)(netId.id - 1);
            if (num >= 0 && num < idMinusOneToName.Count)
            {
                return idMinusOneToName[num];
            }
        }
        return null;
    }

    /// <summary>
    /// Called when resetting network state.
    /// </summary>
    internal static void Clear()
    {
        nameToId.Clear();
        idMinusOneToName.Clear();
    }

    /// <summary>
    /// Called on server and singleplayer before loading level.
    /// </summary>
    internal static void ServerPopulateTable()
    {
        foreach (KeyValuePair<Guid, PhysicsMaterialAsset> asset in PhysicMaterialCustomData.GetAssets())
        {
            string[] physicMaterialNames = asset.Value.physicMaterialNames;
            foreach (string text in physicMaterialNames)
            {
                if (!nameToId.ContainsKey(text))
                {
                    uint value = (uint)(idMinusOneToName.Count + 1);
                    nameToId[text] = value;
                    idMinusOneToName.Add(text);
                    if (idMinusOneToName.Count == 63)
                    {
                        UnturnedLog.warn($"Reached maximum number of physics material table entries! ({63})");
                        return;
                    }
                }
            }
        }
    }

    internal static void Send(ITransportConnection transportConnection)
    {
        SendMappings.Invoke(ENetReliability.Reliable, transportConnection, delegate(NetPakWriter writer)
        {
            writer.WriteUInt8((byte)idMinusOneToName.Count);
            foreach (string item in idMinusOneToName)
            {
                writer.WriteString(item, 6);
            }
        });
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveMappings(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        for (int i = 0; i < value; i++)
        {
            reader.ReadString(out var value2, 6);
            uint value3 = (uint)(idMinusOneToName.Count + 1);
            nameToId[value2] = value3;
            idMinusOneToName.Add(value2);
        }
    }
}
