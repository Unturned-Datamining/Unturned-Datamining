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

    private const byte SAVEDATA_VERSION_NEWEST = 13;

    public static readonly byte SAVEDATA_VERSION = 13;

    public static string connectIP;

    public static ushort connectPort;

    public static string connectPassword;

    public static string serversPassword;

    public static EGameMode singleplayerMode;

    public static bool singleplayerCheats;

    public static string singleplayerMap;

    public static string editorMap;

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
                    connectIP = block.readString();
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
                    if (b > 4)
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
                    return;
                }
            }
        }
        connectIP = "127.0.0.1";
        connectPort = 27015;
        connectPassword = "";
        serversPassword = string.Empty;
        singleplayerMode = EGameMode.NORMAL;
        singleplayerCheats = false;
        singleplayerMap = "";
        editorMap = "";
        singleplayerCategory = ESingleplayerMapCategory.OFFICIAL;
    }

    public static void save()
    {
        Block block = new Block();
        block.writeByte(13);
        block.writeString(connectIP);
        block.writeUInt16(connectPort);
        block.writeString(connectPassword);
        block.writeString(serversPassword);
        block.writeByte((byte)singleplayerMode);
        block.writeBoolean(singleplayerCheats);
        block.writeString(singleplayerMap);
        block.writeString(editorMap);
        block.writeBoolean(isVR);
        block.writeByte((byte)singleplayerCategory);
        ReadWrite.writeBlock("/Play.dat", useCloud: true, block);
    }
}
