using System.Collections.Generic;
using SDG.NetPak;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

internal static class ClientMessageHandler_PlayerConnected
{
    private static List<int> skinItems = new List<int>();

    private static List<string> skinTags = new List<string>();

    private static List<string> skinDynamicProps = new List<string>();

    private static readonly NetLength MAX_LENGTH = new NetLength(255u);

    internal static void ReadMessage(NetPakReader reader)
    {
        reader.ReadNetId(out var value);
        reader.ReadSteamID(out CSteamID value2);
        reader.ReadUInt8(out var value3);
        reader.ReadString(out var value4);
        reader.ReadString(out var value5);
        reader.ReadClampedVector3(out var value6);
        reader.ReadUInt8(out var value7);
        reader.ReadBit(out var value8);
        reader.ReadBit(out var value9);
        reader.ReadUInt8(out var value10);
        reader.ReadSteamID(out CSteamID value11);
        reader.ReadString(out var value12);
        reader.ReadUInt8(out var value13);
        reader.ReadUInt8(out var value14);
        reader.ReadUInt8(out var value15);
        reader.ReadColor32RGB(out Color32 value16);
        reader.ReadColor32RGB(out Color32 value17);
        reader.ReadColor32RGB(out Color32 value18);
        reader.ReadBit(out var value19);
        reader.ReadInt32(out var value20);
        reader.ReadInt32(out var value21);
        reader.ReadInt32(out var value22);
        reader.ReadInt32(out var value23);
        reader.ReadInt32(out var value24);
        reader.ReadInt32(out var value25);
        reader.ReadInt32(out var value26);
        skinItems.Clear();
        reader.ReadList(skinItems, delegate(out int item)
        {
            return reader.ReadInt32(out item);
        }, MAX_LENGTH);
        skinTags.Clear();
        reader.ReadList(skinTags, delegate(out string tag)
        {
            return reader.ReadString(out tag);
        }, MAX_LENGTH);
        skinDynamicProps.Clear();
        reader.ReadList(skinDynamicProps, delegate(out string dynProp)
        {
            return reader.ReadString(out dynProp);
        }, MAX_LENGTH);
        reader.ReadEnum(out var value27);
        reader.ReadString(out var value28);
        Provider.addPlayer(null, value, new SteamPlayerID(value2, value3, value4, value5, value12, value11), value6, value7, value8, value9, value10, value13, value14, value15, value16, value17, value18, value19, value20, value21, value22, value23, value24, value25, value26, skinItems.ToArray(), skinTags.ToArray(), skinDynamicProps.ToArray(), value27, value28, CSteamID.Nil, EClientPlatform.Windows).player.InitializePlayer();
    }
}
