namespace SDG.Unturned;

public class ConfigData
{
    public BrowserConfigData Browser;

    public ServerConfigData Server;

    public UnityEventConfigData UnityEvents;

    public ModeConfigData Easy;

    public ModeConfigData Normal;

    public ModeConfigData Hard;

    private ConfigData()
    {
        Browser = new BrowserConfigData();
        Server = new ServerConfigData();
        UnityEvents = new UnityEventConfigData();
        Easy = new ModeConfigData(EGameMode.EASY);
        Normal = new ModeConfigData(EGameMode.NORMAL);
        Hard = new ModeConfigData(EGameMode.HARD);
    }

    public void InitSingleplayerDefaults()
    {
        Easy.InitSingleplayerDefaults();
        Normal.InitSingleplayerDefaults();
        Hard.InitSingleplayerDefaults();
    }

    public void InitDedicatedServerDefaults()
    {
    }

    public ModeConfigData getModeConfig(EGameMode mode)
    {
        return mode switch
        {
            EGameMode.EASY => Easy, 
            EGameMode.NORMAL => Normal, 
            EGameMode.HARD => Hard, 
            _ => null, 
        };
    }

    public static ConfigData CreateDefault(bool singleplayer)
    {
        ConfigData configData = new ConfigData();
        if (singleplayer)
        {
            configData.InitSingleplayerDefaults();
        }
        else
        {
            configData.InitDedicatedServerDefaults();
        }
        return configData;
    }
}
