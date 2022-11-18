using System;
using System.Diagnostics;
using Steamworks;

namespace SDG.Unturned;

public class DedicatedWorkshopUpdateMonitor : IDedicatedWorkshopUpdateMonitor
{
    public struct MonitoredItem
    {
        public PublishedFileId_t fileId;

        public uint initialTimestamp;
    }

    protected bool shouldDoFinalTick;

    protected bool isFinishedTicking;

    protected MonitoredItem[] monitoredItems;

    protected PublishedFileId_t[] fileIdsForQuery;

    protected float queryInterval;

    protected float queryTimer;

    private CallResult<SteamUGCQueryCompleted_t> queryCompleted;

    private UGCQueryHandle_t queryHandle;

    public DedicatedWorkshopUpdateMonitor(MonitoredItem[] monitoredItems, float queryInterval = 900f)
    {
        this.monitoredItems = monitoredItems;
        fileIdsForQuery = new PublishedFileId_t[monitoredItems.Length];
        for (int i = 0; i < monitoredItems.Length; i++)
        {
            fileIdsForQuery[i] = monitoredItems[i].fileId;
        }
        this.queryInterval = queryInterval;
        queryTimer = 0f;
        queryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create(onQueryCompleted);
        queryHandle = UGCQueryHandle_t.Invalid;
    }

    public void tick(float deltaTime)
    {
        if (isFinishedTicking)
        {
            return;
        }
        if (shouldDoFinalTick)
        {
            shouldDoFinalTick = false;
            isFinishedTicking = true;
            handleFinalTick();
        }
        else if (!(queryHandle != UGCQueryHandle_t.Invalid))
        {
            queryTimer += deltaTime;
            if (queryTimer > queryInterval)
            {
                queryTimer = 0f;
                handleTimerTriggered();
            }
        }
    }

    protected void submitQueryRequest(PublishedFileId_t[] fileIds)
    {
        if (fileIds != null && fileIds.Length >= 1)
        {
            if (queryHandle != UGCQueryHandle_t.Invalid)
            {
                throw new Exception("Already waiting on a pending query");
            }
            queryHandle = SteamGameServerUGC.CreateQueryUGCDetailsRequest(fileIds, (uint)fileIds.Length);
            SteamAPICall_t hAPICall = SteamGameServerUGC.SendQueryUGCRequest(queryHandle);
            queryCompleted.Set(hAPICall);
        }
    }

    protected virtual void handleFinalTick()
    {
        WorkshopDownloadConfig workshopDownloadConfig = WorkshopDownloadConfig.get();
        ChatManager.say(string.Format(workshopDownloadConfig.Shutdown_Update_Detected_Message, workshopDownloadConfig.Shutdown_Update_Detected_Timer), ChatManager.welcomeColor, isRich: true);
        Provider.shutdown(workshopDownloadConfig.Shutdown_Update_Detected_Timer, workshopDownloadConfig.Shutdown_Kick_Message);
    }

    protected virtual void handleUpdateDetected(SteamUGCDetails_t fileDetails)
    {
        CommandWindow.LogFormat("Detected an update to '{0}' ({1})", fileDetails.m_rgchTitle, fileDetails.m_nPublishedFileId);
        shouldDoFinalTick = true;
    }

    protected virtual void handleQueryResponse(SteamUGCQueryCompleted_t callback)
    {
        for (uint num = 0u; num < callback.m_unNumResultsReturned; num++)
        {
            if (!SteamGameServerUGC.GetQueryUGCResult(callback.m_handle, num, out var pDetails))
            {
                continue;
            }
            if (getInitialTimestamp(pDetails.m_nPublishedFileId, out var timestamp))
            {
                if (pDetails.m_rtimeUpdated > timestamp)
                {
                    handleUpdateDetected(pDetails);
                }
            }
            else
            {
                UnturnedLog.warn("Unable to find local timestamp for monitored workshop item '{0}' ({1})", pDetails.m_rgchTitle, pDetails.m_nPublishedFileId);
            }
        }
    }

    protected virtual void handleTimerTriggered()
    {
        submitQueryRequest(fileIdsForQuery);
    }

    protected bool getInitialTimestamp(PublishedFileId_t fileId, out uint timestamp)
    {
        MonitoredItem[] array = monitoredItems;
        for (int i = 0; i < array.Length; i++)
        {
            MonitoredItem monitoredItem = array[i];
            if (monitoredItem.fileId == fileId)
            {
                timestamp = monitoredItem.initialTimestamp;
                return true;
            }
        }
        timestamp = 0u;
        return false;
    }

    private void onQueryCompleted(SteamUGCQueryCompleted_t callback, bool ioFailure)
    {
        if (!(callback.m_handle != queryHandle))
        {
            if (ioFailure)
            {
                UnturnedLog.warn("Encountered IO error when monitoring workshop changes");
            }
            else if (callback.m_eResult == EResult.k_EResultOK)
            {
                handleQueryResponse(callback);
            }
            else
            {
                UnturnedLog.warn("Encountered error '{0}' when monitoring workshop changes", callback.m_eResult);
            }
            SteamGameServerUGC.ReleaseQueryUGCRequest(queryHandle);
            queryHandle = UGCQueryHandle_t.Invalid;
        }
    }

    [Conditional("LOG_WORKSHOP_UPDATE_MONITOR")]
    private void debugMessage(string message)
    {
        CommandWindow.Log(message);
    }

    [Conditional("LOG_WORKSHOP_UPDATE_MONITOR")]
    private void debugMessage(string format, params object[] args)
    {
        CommandWindow.LogFormat(format, args);
    }
}
