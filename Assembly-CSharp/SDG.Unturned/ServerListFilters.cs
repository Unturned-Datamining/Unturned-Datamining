using System.Collections.Generic;

namespace SDG.Unturned;

public class ServerListFilters
{
    public string presetName = string.Empty;

    /// <summary>
    /// Assigned when a named preset is created.
    /// 0 is the default and should be replaced by a preset when loaded.
    /// -1 indicates the preset was modified.
    /// -2 and below are the default presets.
    /// </summary>
    public int presetId;

    public string serverName = string.Empty;

    public HashSet<string> mapNames = new HashSet<string>();

    public EPassword password;

    public EWorkshop workshop = EWorkshop.ANY;

    public EPlugins plugins = EPlugins.ANY;

    public EAttendance attendance = EAttendance.HasPlayers;

    /// <summary>
    /// If true, only servers with available player slots are shown.
    /// </summary>
    public bool notFull = true;

    public EVACProtectionFilter vacProtection;

    public EBattlEyeProtectionFilter battlEyeProtection;

    public ECombat combat = ECombat.ANY;

    public ECheats cheats;

    public ECameraMode camera = ECameraMode.ANY;

    public EServerMonetizationTag monetization = EServerMonetizationTag.Any;

    public EServerListGoldFilter gold;

    public ESteamServerList listSource;

    /// <summary>
    /// If &gt;0, servers with ping higher than this will not be shown.
    /// </summary>
    public int maxPing = 300;

    public void GetLevels(List<LevelInfo> levels)
    {
        foreach (string mapName in mapNames)
        {
            LevelInfo levelInfo = Level.FindLevelForServerFilterExact(mapName);
            if (levelInfo != null)
            {
                levels.Add(levelInfo);
            }
        }
    }

    public string GetMapDisplayText()
    {
        string text = string.Empty;
        foreach (string mapName in mapNames)
        {
            if (text.Length > 0)
            {
                text += ", ";
            }
            LevelInfo levelInfo = Level.FindLevelForServerFilterExact(mapName);
            text = ((levelInfo == null) ? (text + mapName) : (text + levelInfo.getLocalizedName()));
        }
        return text;
    }

    /// <returns>True if level was added to the list of maps.</returns>
    public bool ToggleMap(LevelInfo levelInfo)
    {
        if (levelInfo == null)
        {
            return false;
        }
        string item = levelInfo.name.ToLower();
        if (!mapNames.Remove(item))
        {
            mapNames.Add(item);
            return true;
        }
        return false;
    }

    public void ClearMaps()
    {
        mapNames.Clear();
    }

    public void CopyFrom(ServerListFilters source)
    {
        presetName = source.presetName;
        presetId = source.presetId;
        serverName = source.serverName;
        mapNames = new HashSet<string>(source.mapNames);
        password = source.password;
        workshop = source.workshop;
        plugins = source.plugins;
        attendance = source.attendance;
        notFull = source.notFull;
        vacProtection = source.vacProtection;
        battlEyeProtection = source.battlEyeProtection;
        combat = source.combat;
        cheats = source.cheats;
        camera = source.camera;
        monetization = source.monetization;
        gold = source.gold;
        listSource = source.listSource;
        maxPing = source.maxPing;
    }

    public void Read(byte version, Block block)
    {
        if (version >= 20)
        {
            int num = block.readInt32();
            for (int i = 0; i < num; i++)
            {
                string text = block.readString();
                if (!string.IsNullOrEmpty(text))
                {
                    mapNames.Add(text);
                }
            }
        }
        else
        {
            string text2 = block.readString();
            if (!string.IsNullOrEmpty(text2))
            {
                mapNames.Add(text2);
            }
        }
        password = (EPassword)block.readByte();
        workshop = (EWorkshop)block.readByte();
        plugins = (EPlugins)block.readByte();
        attendance = (EAttendance)block.readByte();
        notFull = block.readBoolean();
        vacProtection = (EVACProtectionFilter)block.readByte();
        battlEyeProtection = (EBattlEyeProtectionFilter)block.readByte();
        combat = (ECombat)block.readByte();
        cheats = (ECheats)block.readByte();
        camera = (ECameraMode)block.readByte();
        monetization = (EServerMonetizationTag)block.readByte();
        gold = (EServerListGoldFilter)block.readByte();
        serverName = block.readString();
        listSource = (ESteamServerList)block.readByte();
        presetName = block.readString();
        presetId = block.readInt32();
        if (version >= 22)
        {
            maxPing = block.readInt32();
            if (version < 24 && maxPing == 200)
            {
                maxPing = 300;
            }
        }
        else
        {
            maxPing = 300;
        }
    }

    public void Write(Block block)
    {
        block.writeInt32(mapNames.Count);
        foreach (string mapName in mapNames)
        {
            block.writeString(mapName);
        }
        block.writeByte((byte)password);
        block.writeByte((byte)workshop);
        block.writeByte((byte)plugins);
        block.writeByte((byte)attendance);
        block.writeBoolean(notFull);
        block.writeByte((byte)vacProtection);
        block.writeByte((byte)battlEyeProtection);
        block.writeByte((byte)combat);
        block.writeByte((byte)cheats);
        block.writeByte((byte)camera);
        block.writeByte((byte)monetization);
        block.writeByte((byte)gold);
        block.writeString(serverName);
        block.writeByte((byte)listSource);
        block.writeString(presetName);
        block.writeInt32(presetId);
        block.writeInt32(maxPing);
    }
}
