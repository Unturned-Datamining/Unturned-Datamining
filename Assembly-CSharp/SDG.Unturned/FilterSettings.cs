using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public static class FilterSettings
{
    public class ServerBrowserColumns
    {
        public bool map = true;

        public bool players = true;

        public bool maxPlayers;

        public bool ping = true;

        public bool anticheat;

        public bool perspective;

        public bool combat;

        public bool password;

        public bool workshop;

        public bool gold;

        public bool cheats;

        public bool monetization;

        public bool plugins;

        public void Read(Block block)
        {
            map = block.readBoolean();
            players = block.readBoolean();
            maxPlayers = block.readBoolean();
            ping = block.readBoolean();
            anticheat = block.readBoolean();
            perspective = block.readBoolean();
            combat = block.readBoolean();
            password = block.readBoolean();
            workshop = block.readBoolean();
            gold = block.readBoolean();
            cheats = block.readBoolean();
            monetization = block.readBoolean();
            plugins = block.readBoolean();
        }

        public void Write(Block block)
        {
            block.writeBoolean(map);
            block.writeBoolean(players);
            block.writeBoolean(maxPlayers);
            block.writeBoolean(ping);
            block.writeBoolean(anticheat);
            block.writeBoolean(perspective);
            block.writeBoolean(combat);
            block.writeBoolean(password);
            block.writeBoolean(workshop);
            block.writeBoolean(gold);
            block.writeBoolean(cheats);
            block.writeBoolean(monetization);
            block.writeBoolean(plugins);
        }
    }

    /// <summary>
    /// Version before named version constants were introduced. (2023-11-13)
    /// </summary>
    public const byte SAVEDATA_VERSION_INITIAL = 14;

    public const byte SAVEDATA_VERSION_ADDED_GOLD_FILTER = 15;

    public const byte SAVEDATA_VERSION_MOVED_SERVER_NAME_FILTER = 16;

    public const byte SAVEDATA_VERSION_ADDED_PRESETS = 17;

    public const byte SAVEDATA_VERSION_SAVE_COLUMNS = 18;

    public const byte SAVEDATA_VERSION_SAVE_SUBMENUS_OPEN = 19;

    private const byte SAVEDATA_VERSION_NEWEST = 19;

    public static readonly byte SAVEDATA_VERSION;

    public static ServerListFilters activeFilters;

    public static bool isColumnsEditorOpen;

    public static bool isPresetsListOpen;

    public static bool isQuickFiltersEditorOpen;

    public static List<ServerListFilters> customPresets;

    private static int nextCustomPresetId;

    public static ServerListFilters defaultPresetInternet;

    public static ServerListFilters defaultPresetLAN;

    public static ServerListFilters defaultPresetHistory;

    public static ServerListFilters defaultPresetFavorites;

    public static ServerListFilters defaultPresetFriends;

    public static ServerBrowserColumns columns;

    public static event System.Action OnActiveFiltersModified;

    public static event System.Action OnActiveFiltersReplaced;

    public static event System.Action OnCustomPresetsListChanged;

    public static void InvokeActiveFiltersReplaced()
    {
        FilterSettings.OnActiveFiltersReplaced?.Invoke();
    }

    public static void InvokeCustomFiltersListChanged()
    {
        FilterSettings.OnCustomPresetsListChanged?.Invoke();
    }

    public static int CreatePresetId()
    {
        int result = nextCustomPresetId;
        nextCustomPresetId++;
        return result;
    }

    public static void RemovePreset(int presetId)
    {
        for (int num = customPresets.Count - 1; num >= 0; num--)
        {
            if (customPresets[num].presetId == presetId)
            {
                customPresets.RemoveAt(num);
            }
        }
    }

    public static void MarkActiveFilterModified()
    {
        if (activeFilters.presetId != -1)
        {
            if (!string.IsNullOrEmpty(activeFilters.presetName))
            {
                activeFilters.presetName = MenuPlayUI.serverListUI.localization.format("PresetName_Modified", activeFilters.presetName);
            }
            activeFilters.presetId = -1;
            FilterSettings.OnActiveFiltersModified?.Invoke();
        }
    }

    public static void load()
    {
        if (ReadWrite.fileExists("/Filters.dat", useCloud: true))
        {
            Block block = ReadWrite.readBlock("/Filters.dat", useCloud: true, 0);
            if (block != null)
            {
                byte b = block.readByte();
                if (b > 2)
                {
                    activeFilters.mapName = block.readString();
                    if (b > 5)
                    {
                        activeFilters.password = (EPassword)block.readByte();
                        activeFilters.workshop = (EWorkshop)block.readByte();
                        if (b < 12)
                        {
                            activeFilters.workshop = EWorkshop.ANY;
                        }
                    }
                    else
                    {
                        block.readBoolean();
                        block.readBoolean();
                        activeFilters.password = EPassword.NO;
                        activeFilters.workshop = EWorkshop.ANY;
                    }
                    if (b < 7)
                    {
                        activeFilters.plugins = EPlugins.ANY;
                    }
                    else
                    {
                        activeFilters.plugins = (EPlugins)block.readByte();
                    }
                    activeFilters.attendance = (EAttendance)block.readByte();
                    if (b >= 14)
                    {
                        activeFilters.notFull = block.readBoolean();
                    }
                    else
                    {
                        activeFilters.notFull = true;
                    }
                    activeFilters.vacProtection = (EVACProtectionFilter)block.readByte();
                    if (b > 10)
                    {
                        activeFilters.battlEyeProtection = (EBattlEyeProtectionFilter)block.readByte();
                    }
                    else
                    {
                        activeFilters.battlEyeProtection = EBattlEyeProtectionFilter.Secure;
                    }
                    activeFilters.combat = (ECombat)block.readByte();
                    if (b < 8)
                    {
                        activeFilters.cheats = ECheats.NO;
                    }
                    else
                    {
                        activeFilters.cheats = (ECheats)block.readByte();
                    }
                    if (b < 15)
                    {
                        block.readByte();
                    }
                    if (b > 3)
                    {
                        activeFilters.camera = (ECameraMode)block.readByte();
                    }
                    else
                    {
                        activeFilters.camera = ECameraMode.ANY;
                    }
                    if (b >= 13)
                    {
                        activeFilters.monetization = (EServerMonetizationTag)block.readByte();
                    }
                    else
                    {
                        activeFilters.monetization = EServerMonetizationTag.Any;
                    }
                    if (b >= 15)
                    {
                        activeFilters.gold = (EServerListGoldFilter)block.readByte();
                    }
                    else
                    {
                        activeFilters.gold = EServerListGoldFilter.Any;
                    }
                    if (b >= 16)
                    {
                        activeFilters.serverName = block.readString();
                    }
                    else
                    {
                        activeFilters.serverName = string.Empty;
                    }
                    if (b >= 17)
                    {
                        activeFilters.listSource = (ESteamServerList)block.readByte();
                        activeFilters.presetName = block.readString();
                        activeFilters.presetId = block.readInt32();
                    }
                    else
                    {
                        activeFilters.listSource = ESteamServerList.INTERNET;
                        activeFilters.presetName = string.Empty;
                        activeFilters.presetId = -1;
                    }
                    if (b >= 17)
                    {
                        nextCustomPresetId = block.readInt32();
                        int num = block.readInt32();
                        for (int i = 0; i < num; i++)
                        {
                            ServerListFilters serverListFilters = new ServerListFilters();
                            serverListFilters.Read(block);
                            customPresets.Add(serverListFilters);
                        }
                    }
                    else
                    {
                        nextCustomPresetId = 1;
                    }
                    if (b >= 18)
                    {
                        columns.Read(block);
                    }
                    if (b >= 19)
                    {
                        isColumnsEditorOpen = block.readBoolean();
                        isPresetsListOpen = block.readBoolean();
                        isQuickFiltersEditorOpen = block.readBoolean();
                    }
                    else
                    {
                        isColumnsEditorOpen = false;
                        isPresetsListOpen = true;
                        isQuickFiltersEditorOpen = false;
                    }
                    return;
                }
            }
        }
        isPresetsListOpen = true;
    }

    public static void save()
    {
        Block block = new Block();
        block.writeByte(19);
        activeFilters.Write(block);
        block.writeInt32(nextCustomPresetId);
        block.writeInt32(customPresets.Count);
        foreach (ServerListFilters customPreset in customPresets)
        {
            customPreset.Write(block);
        }
        columns.Write(block);
        block.writeBoolean(isColumnsEditorOpen);
        block.writeBoolean(isPresetsListOpen);
        block.writeBoolean(isQuickFiltersEditorOpen);
        ReadWrite.writeBlock("/Filters.dat", useCloud: true, block);
    }

    static FilterSettings()
    {
        SAVEDATA_VERSION = 19;
        activeFilters = new ServerListFilters();
        customPresets = new List<ServerListFilters>();
        nextCustomPresetId = 1;
        defaultPresetInternet = new ServerListFilters();
        defaultPresetLAN = new ServerListFilters();
        defaultPresetHistory = new ServerListFilters();
        defaultPresetFavorites = new ServerListFilters();
        defaultPresetFriends = new ServerListFilters();
        columns = new ServerBrowserColumns();
        defaultPresetInternet.presetId = -2;
        defaultPresetLAN.presetId = -3;
        defaultPresetLAN.listSource = ESteamServerList.LAN;
        defaultPresetLAN.password = EPassword.ANY;
        defaultPresetLAN.vacProtection = EVACProtectionFilter.Any;
        defaultPresetLAN.battlEyeProtection = EBattlEyeProtectionFilter.Any;
        defaultPresetLAN.cheats = ECheats.ANY;
        defaultPresetLAN.attendance = EAttendance.Any;
        defaultPresetLAN.notFull = false;
        defaultPresetHistory.presetId = -4;
        defaultPresetHistory.listSource = ESteamServerList.HISTORY;
        defaultPresetHistory.password = EPassword.ANY;
        defaultPresetHistory.vacProtection = EVACProtectionFilter.Any;
        defaultPresetHistory.battlEyeProtection = EBattlEyeProtectionFilter.Any;
        defaultPresetHistory.cheats = ECheats.ANY;
        defaultPresetHistory.attendance = EAttendance.Any;
        defaultPresetHistory.notFull = false;
        defaultPresetFavorites.presetId = -5;
        defaultPresetFavorites.listSource = ESteamServerList.FAVORITES;
        defaultPresetFavorites.password = EPassword.ANY;
        defaultPresetFavorites.vacProtection = EVACProtectionFilter.Any;
        defaultPresetFavorites.battlEyeProtection = EBattlEyeProtectionFilter.Any;
        defaultPresetFavorites.cheats = ECheats.ANY;
        defaultPresetFavorites.attendance = EAttendance.Any;
        defaultPresetFavorites.notFull = false;
        defaultPresetFriends.presetId = -6;
        defaultPresetFriends.listSource = ESteamServerList.FRIENDS;
        defaultPresetFriends.password = EPassword.ANY;
        defaultPresetFriends.vacProtection = EVACProtectionFilter.Any;
        defaultPresetFriends.battlEyeProtection = EBattlEyeProtectionFilter.Any;
        defaultPresetFriends.cheats = ECheats.ANY;
        defaultPresetFriends.attendance = EAttendance.Any;
        defaultPresetFriends.notFull = false;
    }
}
