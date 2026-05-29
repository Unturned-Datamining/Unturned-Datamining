using System;
using System.IO;

namespace SDG.Unturned;

public static class ThirdpartyAntiCheat
{
    public const string SecureLocalizationKey = "BattlEye_Secure";

    public const string InsecureLocalizationKey = "BattlEye_Insecure";

    public const string CommandLineFlag = "-BattlEye";

    public const string ServerListAnticheatColumnTooltipKey = "Anticheat_Column_BattlEye_Tooltip";

    public const string FilterToggleLabelKey = "BattlEye_Filter_Label";

    public const string FilterToggleTooltipKey = "BattlEye_Filter_Toggle_Tooltip";

    public const string FilterSecureKey = "BattlEye_Secure_Button";

    public const string FilterSecureTooltipKey = "BattlEye_Filter_Secure_Tooltip";

    public const string FilterInsecureKey = "BattlEye_Insecure_Button";

    public const string FilterInsecureTooltipKey = "BattlEye_Filter_Insecure_Tooltip";

    public const string FilterAnyKey = "BattlEye_Any_Button";

    public const string FilterAnyTooltipKey = "BattlEye_Filter_Any_Tooltip";

    public const string IconName = "BattlEye";

    public const string IconInsecureName = "BattlEye_Off";

    public const string MenuHeaderKey = "BattlEye_Header";

    public const string MenuBodyKey = "BattlEye_Body";

    public const string AdvertisementMismatchKey = "Server_BattlEye_Advertisement_Mismatch";

    public const string DisconnectBrokenKey = "BattlEye_Broken";

    public const string DisconnectUpdateKey = "BattlEye_Update";

    public const string DisconnectUnknownKey = "BattlEye_Unknown";

    public const string GamebanSuffix = " was banned by BattlEye";

    public const string ExtraFilesDirName = "BattlEye";

    public static void OpenDirectory()
    {
        try
        {
            ReadWrite.OpenFileBrowser(new DirectoryInfo(ReadWrite.PATH).CreateSubdirectory("BattlEye").FullName);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Exception opening BattlEye folder");
        }
    }
}
