using System;

namespace SDG.Unturned;

public class ConfigData
{
    public BrowserConfigData Browser;

    public ServerConfigData Server;

    public UnityEventConfigData UnityEvents;

    [Obsolete("Please update code to no longer reference this directly! The intention is to stop storing all three side-by-side in the future.")]
    public ModeConfigData Easy;

    [Obsolete("Please update code to no longer reference this directly! The intention is to stop storing all three side-by-side in the future.")]
    public ModeConfigData Normal;

    [Obsolete("Please update code to no longer reference this directly! The intention is to stop storing all three side-by-side in the future.")]
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
