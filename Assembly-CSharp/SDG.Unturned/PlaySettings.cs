using System;

namespace SDG.Unturned;

public class PlaySettings
{
    /// <summary>
    /// Version before named version constants were introduced. (2023-11-08)
    /// </summary>
    public const byte SAVEDATA_VERSION_INITIAL = 11;

    public const byte SAVEDATA_VERSION_REMOVED_MATCHMAKING = 12;

    /// <summary>
    /// Moved into ServerListFilters.
    /// </summary>
    public const byte SAVEDATA_VERSION_REMOVED_SERVER_NAME_FILTER = 13;

    public const byte SAVEDATA_VERSION_PERSIST_LEVEL_WORKSHOP_FILE_ID = 14;

    private const byte SAVEDATA_VERSION_NEWEST = 14;

    public static readonly byte SAVEDATA_VERSION = 14;

    public static string connectHost;

    public static ushort connectPort;

    public static string connectPassword;

    public static string serversPassword;

    public static EGameMode singleplayerMode;

    public static bool singleplayerCheats;

    [Obsolete]
    public static string singleplayerMap;

    [Obsolete]
    public static string editorMap;

    internal static SavedLevelSelection singleplayerLevelSelection;

    internal static SavedLevelSelection editorLevelSelection;

    public static bool isVR;

    public static ESingleplayerMapCategory singleplayerCategory;

    public static void load()
    {
        if (ReadWrite.fileExists("/Play.dat", useCloud: true))
        {
            Block block = ReadWrite.readBlock("/Play.dat", useCloud: true, 0);
            if (block != null)
            {
                byte b = block.readByte();
                if (b > 1)
                {
                    connectHost = block.readString();
                    connectPort = block.readUInt16();
                    connectPassword = block.readString();
                    if (b > 3 && b < 13)
                    {
                        block.readString();
                    }
                    serversPassword = block.readString();
                    singleplayerMode = (EGameMode)block.readByte();
                    if (b < 8)
                    {
                        singleplayerMode = EGameMode.NORMAL;
                    }
                    if (b > 10 && b < 12)
                    {
                        block.readByte();
                    }
                    if (b < 7)
                    {
                        singleplayerCheats = false;
                    }
                    else
                    {
                        singleplayerCheats = block.readBoolean();
                    }
                    if (b > 4 && b < 14)
                    {
                        singleplayerMap = block.readString();
                        editorMap = block.readString();
                    }
                    else
                    {
                        singleplayerMap = "";
                        editorMap = "";
                    }
                    if (b > 10 && b < 12)
                    {
                        block.readString();
                    }
                    if (b > 5)
                    {
                        isVR = block.readBoolean();
                        if (b < 9)
                        {
                            isVR = false;
                        }
                    }
                    else
                    {
                        isVR = false;
                    }
                    if (b < 10)
                    {
                        singleplayerCategory = ESingleplayerMapCategory.OFFICIAL;
                    }
                    else
                    {
                        singleplayerCategory = (ESingleplayerMapCategory)block.readByte();
                    }
                    if (b >= 14)
                    {
                        singleplayerLevelSelection.Read(block);
                        editorLevelSelection.Read(block);
                    }
                    else
                    {
                        singleplayerLevelSelection.name = singleplayerMap;
                        editorLevelSelection.name = editorMap;
                    }
                    return;
                }
            }
        }
        connectHost = "127.0.0.1";
        connectPort = 27015;
        connectPassword = "";
        serversPassword = string.Empty;
        singleplayerMode = EGameMode.NORMAL;
        singleplayerCheats = false;
        singleplayerMap = "";
        editorMap = "";
        singleplayerLevelSelection.name = string.Empty;
        editorLevelSelection.name = string.Empty;
        singleplayerCategory = ESingleplayerMapCategory.OFFICIAL;
    }

    public static void save()
    {
        Block block = new Block();
        block.writeByte(14);
        block.writeString(connectHost);
        block.writeUInt16(connectPort);
        block.writeString(connectPassword);
        block.writeString(serversPassword);
        block.writeByte((byte)singleplayerMode);
        block.writeBoolean(singleplayerCheats);
        block.writeBoolean(isVR);
        block.writeByte((byte)singleplayerCategory);
        singleplayerLevelSelection.Write(block);
        editorLevelSelection.Write(block);
        ReadWrite.writeBlock("/Play.dat", useCloud: true, block);
    }
}
