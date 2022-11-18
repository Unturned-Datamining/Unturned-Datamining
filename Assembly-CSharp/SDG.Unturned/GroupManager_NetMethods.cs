using SDG.NetPak;
using Steamworks;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(GroupManager))]
public static class GroupManager_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveGroupInfo", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveGroupInfo_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadSteamID(out CSteamID value);
        reader.ReadString(out var value2);
        reader.ReadUInt32(out var value3);
        GroupManager.ReceiveGroupInfo(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveGroupInfo", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveGroupInfo_Write(NetPakWriter writer, CSteamID groupID, string name, uint members)
    {
        writer.WriteSteamID(groupID);
        writer.WriteString(name);
        writer.WriteUInt32(members);
    }
}
