using System;
using System.Collections.Generic;

namespace SDG.Unturned;

/// <summary>
/// Configuration for DedicatedUGC.
/// </summary>
public class WorkshopDownloadConfig
{
    /// <summary>
    /// Published workshop file IDs to download.
    /// </summary>
    public List<ulong> File_IDs;

    /// <summary>
    /// Published workshop file IDs whose children (dependencies) should be skipped.
    /// Useful if workshop author lists dependencies as a way of advertising.
    /// </summary>
    public List<ulong> Ignore_Children_File_IDs;

    /// <summary>
    /// Controls SetAllowCachedResponse. Disabled when set to zero.
    /// Balance between item change frequency and allowing cached results when query fails.
    /// </summary>
    public uint Query_Cache_Max_Age_Seconds;

    /// <summary>
    /// Number of total times to try re-submitting failed workshop queries before aborting.
    /// </summary>
    public uint Max_Query_Retries;

    /// <summary>
    /// Should items already installed be loaded?
    /// </summary>
    public bool Use_Cached_Downloads;

    /// <summary>
    /// Should used items be monitored for updates?
    /// </summary>
    public bool Should_Monitor_Updates;

    /// <summary>
    /// Seconds to wait before shutting down after an update is detected.
    /// </summary>
    public int Shutdown_Update_Detected_Timer;

    /// <summary>
    /// Message broadcasted when shutdown timer begins.
    /// </summary>
    public string Shutdown_Update_Detected_Message;

    /// <summary>
    /// Message sent to players when shutdown timer completes.
    /// </summary>
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

    /// <summary>
    /// Get instance if loaded, but do not load.
    /// </summary>
    public static WorkshopDownloadConfig get()
    {
        return instance;
    }

    /// <summary>
    /// Get instance, or load if not yet loaded.
    /// </summary>
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
        WorkshopDownloadConfig workshopDownloadConfig;
        try
        {
            workshopDownloadConfig = new WorkshopDownloadConfig();
            workshopDownloadConfig.File_IDs = ServerSavedata.deserializeJSON<List<ulong>>("/WorkshopDownloadIDs.json");
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Unable to parse WorkshopDownloadIDs.json! consider validating with a JSON linter");
            workshopDownloadConfig = null;
        }
        return workshopDownloadConfig;
    }
}
