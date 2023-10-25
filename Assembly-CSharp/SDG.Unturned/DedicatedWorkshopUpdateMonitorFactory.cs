using SDG.Provider;
using Steamworks;

namespace SDG.Unturned;

/// <summary>
/// Static functions for creating monitor instance on server.
/// </summary>
public static class DedicatedWorkshopUpdateMonitorFactory
{
    public delegate IDedicatedWorkshopUpdateMonitor CreateHandler(LevelInfo level);

    public static event CreateHandler onCreateForLevel;

    /// <summary>
    /// Entry point called by dedicated server after loading level.
    /// </summary>
    public static IDedicatedWorkshopUpdateMonitor createForLevel(LevelInfo level)
    {
        if (!WorkshopDownloadConfig.get().Should_Monitor_Updates)
        {
            return null;
        }
        if (DedicatedWorkshopUpdateMonitorFactory.onCreateForLevel == null)
        {
            return createDefaultForLevel(level);
        }
        return DedicatedWorkshopUpdateMonitorFactory.onCreateForLevel(level);
    }

    /// <summary>
    /// Create vanilla update monitor that watches for changes to workshop level file.
    /// </summary>
    public static IDedicatedWorkshopUpdateMonitor createDefaultForLevel(LevelInfo level)
    {
        if (!createMonitoredItemForLevel(level, out var monitoredItem))
        {
            return null;
        }
        CommandWindow.LogFormat("Monitoring workshop map '{0}' ({1}) for changes", level.name, level.publishedFileId);
        return new DedicatedWorkshopUpdateMonitor(new DedicatedWorkshopUpdateMonitor.MonitoredItem[1] { monitoredItem });
    }

    /// <summary>
    /// Helper to get updated timestamp from workshop items loaded by DedicatedUGC.
    /// </summary>
    public static bool getCachedInitialTimestamp(PublishedFileId_t fileId, out uint timestamp)
    {
        if (TempSteamworksWorkshop.getCachedDetails(fileId, out var cachedDetails))
        {
            timestamp = cachedDetails.updateTimestamp;
            return true;
        }
        timestamp = 0u;
        return false;
    }

    /// <summary>
    /// Helper to create monitored item for use with default DedicatedWorkshopUpdateMonitor implementation.
    /// </summary>
    public static bool createMonitoredItem(PublishedFileId_t fileId, out DedicatedWorkshopUpdateMonitor.MonitoredItem monitoredItem)
    {
        if (getCachedInitialTimestamp(fileId, out var timestamp))
        {
            monitoredItem = default(DedicatedWorkshopUpdateMonitor.MonitoredItem);
            monitoredItem.fileId = fileId;
            monitoredItem.initialTimestamp = timestamp;
            return true;
        }
        monitoredItem = default(DedicatedWorkshopUpdateMonitor.MonitoredItem);
        return false;
    }

    /// <summary>
    /// For use with default DedicatedWorkshopUpdateMonitor implementation.
    /// </summary>
    public static bool createMonitoredItemForLevel(LevelInfo level, out DedicatedWorkshopUpdateMonitor.MonitoredItem monitoredItem)
    {
        if (level != null && level.isFromWorkshop)
        {
            PublishedFileId_t publishedFileId_t = new PublishedFileId_t(level.publishedFileId);
            if (createMonitoredItem(publishedFileId_t, out monitoredItem))
            {
                return true;
            }
            CommandWindow.LogWarningFormat("Unable to monitor level '{0}' ({1}) for changes because no details were cached", level.name, publishedFileId_t);
        }
        monitoredItem = default(DedicatedWorkshopUpdateMonitor.MonitoredItem);
        return false;
    }
}
