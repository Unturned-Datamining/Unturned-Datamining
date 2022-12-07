using System;

namespace SDG.Unturned;

[Flags]
public enum EPluginWidgetFlags
{
    None = 0,
    Modal = 1,
    NoBlur = 2,
    ForceBlur = 4,
    ShowInteractWithEnemy = 8,
    ShowDeathMenu = 0x10,
    ShowHealth = 0x20,
    ShowFood = 0x40,
    ShowWater = 0x80,
    ShowVirus = 0x100,
    ShowStamina = 0x200,
    ShowOxygen = 0x400,
    ShowStatusIcons = 0x800,
    ShowUseableGunStatus = 0x1000,
    ShowVehicleStatus = 0x2000,
    ShowCenterDot = 0x4000,
    ShowReputationChangeNotification = 0x8000,
    ShowLifeMeters = 0x7E0,
    Default = 0xFFF8
}
