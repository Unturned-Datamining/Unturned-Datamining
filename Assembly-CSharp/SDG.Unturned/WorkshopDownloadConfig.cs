using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public class WorkshopDownloadConfig
{
    public List<ulong> File_IDs;

    public List<ulong> Ignore_Children_File_IDs;

    public uint Query_Cache_Max_Age_Seconds;

    public uint Max_Query_Retries;

    public bool Use_Cached_Downloads;

    public bool Should_Monitor_Updates;

    public int Shutdown_Update_Detected_Timer;

    public string Shutdown_Update_Detected_Message;

    public string Shutdown_Kick_Message;

    private static WorkshopDownloadConfig instance;

    public WorkshopDownloadConfig()
    {
        File_IDs = new List<ulong>();
        Ignore_Children_File_IDs = new List<ulong>();
        Query_Cache_Max_Age_Seconds = 600u;
        Max_Query_Retries = 2u;
        Use_Cached_Downloads = true;
        Should_Monitor_Updates = true;
        Shutdown_Update_Detected_Timer = 600;
        Shutdown_Update_Detected_Message = "Workshop file update detected, shutdown in: {0}";
        Shutdown_Kick_Message = "Shutdown for Workshop file update.";
    }

    public static WorkshopDownloadConfig get()
    {
        return instance;
    }

    public static WorkshopDownloadConfig getOrLoad()
    {
        if (instance == null)
        {
            instance = load();
        }
        return instance;
    }

    private static WorkshopDownloadConfig load()
    {
        WorkshopDownloadConfig workshopDownloadConfig = (ServerSavedata.fileExists("/WorkshopDownloadConfig.json") ? loadFromConfig() : ((!ServerSavedata.fileExists("/WorkshopDownloadIDs.json")) ? null : loadFromLegacyFormat()));
        if (workshopDownloadConfig == null)
        {
            workshopDownloadConfig = new WorkshopDownloadConfig();
        }
        ServerSavedata.serializeJSON("/WorkshopDownloadConfig.json", workshopDownloadConfig);
        return workshopDownloadConfig;
    }

    private static WorkshopDownloadConfig loadFromConfig()
    {
        try
        {
            return ServerSavedata.deserializeJSON<WorkshopDownloadConfig>("/WorkshopDownloadConfig.json");
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Unable to parse WorkshopDownloadConfig.json! consider validating with a JSON linter");
            return null;
        }
    }

    private static WorkshopDownloadConfig loadFromLegacyFormat()
    {
        try
        {
            WorkshopDownloadConfig workshopDownloadConfig = new WorkshopDownloadConfig();
            workshopDownloadConfig.File_IDs = ServerSavedata.deserializeJSON<List<ulong>>("/WorkshopDownloadIDs.json");
            return workshopDownloadConfig;
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Unable to parse WorkshopDownloadIDs.json! consider validating with a JSON linter");
            return null;
        }
    }
}
