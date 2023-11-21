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

    public string mapName = string.Empty;

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

    public void CopyFrom(ServerListFilters source)
    {
        presetName = source.presetName;
        presetId = source.presetId;
        serverName = source.serverName;
        mapName = source.mapName;
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
    }

    public void Read(Block block)
    {
        mapName = block.readString();
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
    }

    public void Write(Block block)
    {
        block.writeString(mapName);
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
    }
}
