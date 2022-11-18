namespace SDG.Unturned;

public class PlaySettings
{
    public static readonly byte SAVEDATA_VERSION = 11;

    public static string connectIP;

    public static ushort connectPort;

    public static string connectPassword;

    public static string serversName;

    public static string serversPassword;

    public static EGameMode singleplayerMode;

    public static EGameMode matchmakingMode;

    public static bool singleplayerCheats;

    public static string singleplayerMap;

    public static string matchmakingMap;

    public static string editorMap;

    public static bool isVR;

    public static ESingleplayerMapCategory singleplayerCategory;

    public static void restoreFilterDefaultValues()
    {
        serversName = string.Empty;
        serversPassword = string.Empty;
    }

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
                    if (b > 3)
                    {
                        serversName = block.readString();
                    }
                    else
                    {
                        serversName = "";
                    }
                    serversPassword = block.readString();
                    singleplayerMode = (EGameMode)block.readByte();
                    if (b < 8)
                    {
                        singleplayerMode = EGameMode.NORMAL;
                    }
                    if (b > 10)
                    {
                        matchmakingMode = (EGameMode)block.readByte();
                    }
                    else
                    {
                        matchmakingMode = EGameMode.NORMAL;
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
                    if (b > 10)
                    {
                        matchmakingMap = block.readString();
                    }
                    else
                    {
                        matchmakingMap = "";
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
        restoreFilterDefaultValues();
        singleplayerMode = EGameMode.NORMAL;
        matchmakingMode = EGameMode.NORMAL;
        singleplayerCheats = false;
        singleplayerMap = "";
        matchmakingMap = "";
        editorMap = "";
        singleplayerCategory = ESingleplayerMapCategory.OFFICIAL;
    }

    public static void save()
    {
        Block block = new Block();
        block.writeByte(SAVEDATA_VERSION);
        block.writeString(connectIP);
        block.writeUInt16(connectPort);
        block.writeString(connectPassword);
        block.writeString(serversName);
        block.writeString(serversPassword);
        block.writeByte((byte)singleplayerMode);
        block.writeByte((byte)matchmakingMode);
        block.writeBoolean(singleplayerCheats);
        block.writeString(singleplayerMap);
        block.writeString(matchmakingMap);
        block.writeString(editorMap);
        block.writeBoolean(isVR);
        block.writeByte((byte)singleplayerCategory);
        ReadWrite.writeBlock("/Play.dat", useCloud: true, block);
    }
}
