using System;
using Steamworks;

namespace SDG.Unturned;

public class SaveManager : SteamCaller
{
    public static SaveHandler onPreSave;

    public static SaveHandler onPostSave;

    private static void broadcastPreSave()
    {
        try
        {
            onPreSave?.Invoke();
        }
        catch (Exception e)
        {
            UnturnedLog.warn("Plugin raised exception during onPreSave:");
            UnturnedLog.exception(e);
        }
    }

    private static void broadcastPostSave()
    {
        try
        {
            onPostSave?.Invoke();
        }
        catch (Exception e)
        {
            UnturnedLog.warn("Plugin raised exception during onPostSave:");
            UnturnedLog.exception(e);
        }
    }

    public static void save()
    {
        if (!Level.isLoaded)
        {
            UnturnedLog.warn("Ignoring request to save before level finished loading");
            return;
        }
        broadcastPreSave();
        if (Level.info != null && Level.info.type == ELevelType.SURVIVAL)
        {
            foreach (SteamPlayer client in Provider.clients)
            {
                if (client != null && !(client.player == null))
                {
                    client.player.save();
                }
            }
            VehicleManager.save();
            BarricadeManager.save();
            StructureManager.save();
            ObjectManager.save();
            LightingManager.save();
            GroupManager.save();
        }
        if (Dedicator.IsDedicatedServer)
        {
            SteamWhitelist.save();
            SteamBlacklist.save();
            SteamAdminlist.save();
        }
        broadcastPostSave();
    }

    private static void onServerShutdown()
    {
        if (Provider.isServer && Level.isLoaded)
        {
            save();
        }
    }

    private static void onServerDisconnected(CSteamID steamID)
    {
        if (Provider.isServer && Level.isLoaded)
        {
            Player player = PlayerTool.getPlayer(steamID);
            if (player != null)
            {
                player.save();
            }
        }
    }

    private void Start()
    {
        Provider.onServerShutdown = (Provider.ServerShutdown)Delegate.Combine(Provider.onServerShutdown, new Provider.ServerShutdown(onServerShutdown));
        Provider.onServerDisconnected = (Provider.ServerDisconnected)Delegate.Combine(Provider.onServerDisconnected, new Provider.ServerDisconnected(onServerDisconnected));
    }
}
