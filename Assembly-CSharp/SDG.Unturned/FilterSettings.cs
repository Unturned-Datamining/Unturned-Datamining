namespace SDG.Unturned;

public class FilterSettings
{
    public static readonly byte SAVEDATA_VERSION = 14;

    public static string filterMap;

    public static EPassword filterPassword;

    public static EWorkshop filterWorkshop;

    public static EPlugins filterPlugins;

    public static EAttendance filterAttendance;

    /// <summary>
    /// If true, only servers with available player slots are shown.
    /// </summary>
    public static bool filterNotFull;

    public static EVACProtectionFilter filterVACProtection;

    public static EBattlEyeProtectionFilter filterBattlEyeProtection;

    public static ECombat filterCombat;

    public static ECheats filterCheats;

    public static EGameMode filterMode;

    public static ECameraMode filterCamera;

    public static EServerMonetizationTag filterMonetization;

    public static void restoreDefaultValues()
    {
        filterMap = string.Empty;
        filterPassword = EPassword.NO;
        filterWorkshop = EWorkshop.ANY;
        filterPlugins = EPlugins.ANY;
        filterAttendance = EAttendance.HasPlayers;
        filterNotFull = true;
        filterVACProtection = EVACProtectionFilter.Secure;
        filterBattlEyeProtection = EBattlEyeProtectionFilter.Secure;
        filterCombat = ECombat.ANY;
        filterCheats = ECheats.NO;
        filterMode = EGameMode.NORMAL;
        filterCamera = ECameraMode.ANY;
        filterMonetization = EServerMonetizationTag.Any;
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
                    filterMap = block.readString();
                    if (b > 5)
                    {
                        filterPassword = (EPassword)block.readByte();
                        filterWorkshop = (EWorkshop)block.readByte();
                        if (b < 12)
                        {
                            filterWorkshop = EWorkshop.ANY;
                        }
                    }
                    else
                    {
                        block.readBoolean();
                        block.readBoolean();
                        filterPassword = EPassword.NO;
                        filterWorkshop = EWorkshop.ANY;
                    }
                    if (b < 7)
                    {
                        filterPlugins = EPlugins.ANY;
                    }
                    else
                    {
                        filterPlugins = (EPlugins)block.readByte();
                    }
                    filterAttendance = (EAttendance)block.readByte();
                    if (b >= 14)
                    {
                        filterNotFull = block.readBoolean();
                    }
                    else
                    {
                        filterNotFull = true;
                    }
                    filterVACProtection = (EVACProtectionFilter)block.readByte();
                    if (b > 10)
                    {
                        filterBattlEyeProtection = (EBattlEyeProtectionFilter)block.readByte();
                    }
                    else
                    {
                        filterBattlEyeProtection = EBattlEyeProtectionFilter.Secure;
                    }
                    filterCombat = (ECombat)block.readByte();
                    if (b < 8)
                    {
                        filterCheats = ECheats.NO;
                    }
                    else
                    {
                        filterCheats = (ECheats)block.readByte();
                    }
                    filterMode = (EGameMode)block.readByte();
                    if (b < 9)
                    {
                        filterMode = EGameMode.NORMAL;
                    }
                    if (b > 3)
                    {
                        filterCamera = (ECameraMode)block.readByte();
                    }
                    else
                    {
                        filterCamera = ECameraMode.ANY;
                    }
                    if (b >= 13)
                    {
                        filterMonetization = (EServerMonetizationTag)block.readByte();
                    }
                    else
                    {
                        filterMonetization = EServerMonetizationTag.Any;
                    }
                    return;
                }
            }
        }
        restoreDefaultValues();
    }

    public static void save()
    {
        Block block = new Block();
        block.writeByte(SAVEDATA_VERSION);
        block.writeString(filterMap);
        block.writeByte((byte)filterPassword);
        block.writeByte((byte)filterWorkshop);
        block.writeByte((byte)filterPlugins);
        block.writeByte((byte)filterAttendance);
        block.writeBoolean(filterNotFull);
        block.writeByte((byte)filterVACProtection);
        block.writeByte((byte)filterBattlEyeProtection);
        block.writeByte((byte)filterCombat);
        block.writeByte((byte)filterCheats);
        block.writeByte((byte)filterMode);
        block.writeByte((byte)filterCamera);
        block.writeByte((byte)filterMonetization);
        ReadWrite.writeBlock("/Filters.dat", useCloud: true, block);
    }
}
