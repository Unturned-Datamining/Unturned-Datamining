using System;
using System.Collections.Generic;
using Steamworks;

namespace SDG.Unturned;

/// <summary>
/// Allows player to save server advertisement to join again later. Semi-replacement for Steam's built-in favorites
/// and history lists because as of 2024-04-26 they don't seem to work properly with Fake IP.
/// </summary>
internal class ServerBookmarksManager
{
    private List<ServerBookmarkDetails> bookmarkDetails;

    private bool isDirty;

    private static ServerBookmarksManager instance;

    private const string RELATIVE_PATH = "/Cloud/ServerBookmarks.bin";

    public static ServerBookmarkDetails FindBookmarkDetails(CSteamID steamId)
    {
        if (!steamId.BPersistentGameServerAccount())
        {
            return null;
        }
        foreach (ServerBookmarkDetails bookmarkDetail in Get().bookmarkDetails)
        {
            if (bookmarkDetail.steamId == steamId)
            {
                return bookmarkDetail;
            }
        }
        return null;
    }

    /// <returns>details if advertisement is bookmarked.</returns>
    public static ServerBookmarkDetails FindBookmarkDetails(SteamServerAdvertisement advertisement)
    {
        if (!advertisement.steamID.BPersistentGameServerAccount())
        {
            return null;
        }
        ServerBookmarksManager serverBookmarksManager = Get();
        foreach (ServerBookmarkDetails bookmarkDetail in serverBookmarksManager.bookmarkDetails)
        {
            if (bookmarkDetail.steamId == advertisement.steamID)
            {
                bookmarkDetail.UpdateFromAdvertisement(advertisement);
                serverBookmarksManager.isDirty = true;
                return bookmarkDetail;
            }
        }
        return null;
    }

    public static void RemoveBookmark(CSteamID steamId)
    {
        if (!steamId.BPersistentGameServerAccount())
        {
            UnturnedLog.error("Bookmark option should not have been available because server has no ID");
            return;
        }
        ServerBookmarksManager serverBookmarksManager = Get();
        for (int i = 0; i < serverBookmarksManager.bookmarkDetails.Count; i++)
        {
            ServerBookmarkDetails serverBookmarkDetails = serverBookmarksManager.bookmarkDetails[i];
            if (serverBookmarkDetails.steamId == steamId)
            {
                UnturnedLog.info($"Removed server bookmark ({serverBookmarkDetails})");
                serverBookmarksManager.bookmarkDetails.RemoveAt(i);
                serverBookmarksManager.isDirty = true;
                break;
            }
        }
    }

    public static ServerBookmarkDetails AddBookmark(SteamServerAdvertisement advertisement, string bookmarkHost)
    {
        if (!advertisement.steamID.BPersistentGameServerAccount())
        {
            UnturnedLog.error("Bookmark option should not have been available because server has no ID");
            return null;
        }
        if (string.IsNullOrEmpty(bookmarkHost))
        {
            UnturnedLog.error("Bookmark option should not have been available because server has no bookmark host");
            return null;
        }
        ServerBookmarksManager serverBookmarksManager = Get();
        ServerBookmarkDetails serverBookmarkDetails = new ServerBookmarkDetails();
        serverBookmarkDetails.steamId = advertisement.steamID;
        serverBookmarkDetails.host = bookmarkHost;
        serverBookmarkDetails.UpdateFromAdvertisement(advertisement);
        serverBookmarksManager.bookmarkDetails.Add(serverBookmarkDetails);
        serverBookmarksManager.isDirty = true;
        UnturnedLog.info($"Added server bookmark ({serverBookmarkDetails})");
        return serverBookmarkDetails;
    }

    public static ServerBookmarkDetails AddBookmark(CSteamID steamId, string bookmarkHost, ushort queryPort, string name, string description, string thumbnailUrl)
    {
        if (!steamId.BPersistentGameServerAccount())
        {
            UnturnedLog.error("Bookmark option should not have been available because server has no ID");
            return null;
        }
        if (string.IsNullOrEmpty(bookmarkHost))
        {
            UnturnedLog.error("Bookmark option should not have been available because server has no bookmark host");
            return null;
        }
        ServerBookmarksManager serverBookmarksManager = Get();
        ServerBookmarkDetails serverBookmarkDetails = new ServerBookmarkDetails
        {
            steamId = steamId,
            host = bookmarkHost,
            queryPort = queryPort,
            name = name,
            description = description,
            thumbnailUrl = thumbnailUrl
        };
        serverBookmarksManager.bookmarkDetails.Add(serverBookmarkDetails);
        serverBookmarksManager.isDirty = true;
        UnturnedLog.info($"Added server bookmark ({serverBookmarkDetails})");
        return serverBookmarkDetails;
    }

    /// <summary>
    /// Restore a removed bookmark.
    /// </summary>
    public static void AddBookmark(ServerBookmarkDetails details)
    {
        ServerBookmarksManager serverBookmarksManager = Get();
        serverBookmarksManager.bookmarkDetails.Add(details);
        serverBookmarksManager.isDirty = true;
        UnturnedLog.info($"Added server bookmark ({details})");
    }

    public static List<ServerBookmarkDetails> GetList()
    {
        return Get().bookmarkDetails;
    }

    public static void SaveIfDirty()
    {
        if (instance == null)
        {
            UnturnedLog.info("Skipped saving server bookmarks");
        }
        else if (instance.isDirty)
        {
            instance.isDirty = false;
            instance.Save();
            UnturnedLog.info("Saved server bookmarks");
        }
    }

    public static void MarkDirty()
    {
        if (instance != null)
        {
            instance.isDirty = true;
        }
    }

    private static ServerBookmarksManager Get()
    {
        if (instance == null)
        {
            Load();
        }
        return instance;
    }

    private static void Load()
    {
        if (ReadWrite.fileExists("/Cloud/ServerBookmarks.bin", useCloud: false, usePath: true))
        {
            try
            {
                instance = new ServerBookmarksManager();
                River river = new River("/Cloud/ServerBookmarks.bin", usePath: true, useCloud: false, isReading: true);
                river.readByte();
                int num = river.readInt32();
                instance.bookmarkDetails = new List<ServerBookmarkDetails>();
                for (int i = 0; i < num; i++)
                {
                    ServerBookmarkDetails serverBookmarkDetails = new ServerBookmarkDetails();
                    serverBookmarkDetails.steamId = river.readSteamID();
                    serverBookmarkDetails.host = river.readString();
                    serverBookmarkDetails.queryPort = river.readUInt16();
                    serverBookmarkDetails.name = river.readString();
                    serverBookmarkDetails.description = river.readString();
                    serverBookmarkDetails.thumbnailUrl = river.readString();
                    if (!serverBookmarkDetails.steamId.BPersistentGameServerAccount())
                    {
                        UnturnedLog.info($"Discarding server bookmark for \"{serverBookmarkDetails.name}\" at {serverBookmarkDetails.host}:{serverBookmarkDetails.queryPort} because Steam ID ({serverBookmarkDetails.steamId}) is invalid");
                    }
                    else
                    {
                        instance.bookmarkDetails.Add(serverBookmarkDetails);
                    }
                }
                UnturnedLog.info($"Loaded server bookmarks: {num}");
                return;
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, "Caught exception loading server bookmarks:");
                instance = new ServerBookmarksManager();
                instance.Reset();
                return;
            }
        }
        instance = new ServerBookmarksManager();
        instance.Reset();
    }

    private void Save()
    {
        try
        {
            River river = new River("/Cloud/ServerBookmarks.bin", usePath: true, useCloud: false, isReading: false);
            river.writeByte(1);
            river.writeInt32(bookmarkDetails.Count);
            foreach (ServerBookmarkDetails bookmarkDetail in bookmarkDetails)
            {
                river.writeSteamID(bookmarkDetail.steamId);
                river.writeString(bookmarkDetail.host);
                river.writeUInt16(bookmarkDetail.queryPort);
                river.writeString(bookmarkDetail.name);
                river.writeString(bookmarkDetail.description);
                river.writeString(bookmarkDetail.thumbnailUrl);
            }
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception saving server bookmarks:");
        }
    }

    private void Reset()
    {
        bookmarkDetails = new List<ServerBookmarkDetails>();
    }
}
