using System;

namespace SDG.Unturned;

/// <summary>
/// 32-bit mask granting server plugins additional control over custom UIs.
/// Only replicated to owner.
/// </summary>
[Flags]
public enum EPluginWidgetFlags
{
    None = 0,
    /// <summary>
    /// Enables cursor movement while not in a vanilla menu.
    /// </summary>
    Modal = 1,
    /// <summary>
    /// Disable background blur regardless of other UI state.
    /// </summary>
    NoBlur = 2,
    /// <summary>
    /// Enable background blur regardless of other UI state.
    /// Takes precedence over NoBlur.
    /// </summary>
    ForceBlur = 4,
    /// <summary>
    /// Enable title card while focusing a nearby player.
    /// </summary>
    ShowInteractWithEnemy = 8,
    /// <summary>
    /// Enable explanation and respawn buttons while dead.
    /// </summary>
    ShowDeathMenu = 0x10,
    /// <summary>
    /// Enable health meter in the HUD.
    /// </summary>
    ShowHealth = 0x20,
    /// <summary>
    /// Enable food meter in the HUD.
    /// </summary>
    ShowFood = 0x40,
    /// <summary>
    /// Enable water meter in the HUD.
    /// </summary>
    ShowWater = 0x80,
    /// <summary>
    /// Enable virus/radiation/infection meter in the HUD.
    /// </summary>
    ShowVirus = 0x100,
    /// <summary>
    /// Enable stamina meter in the HUD.
    /// </summary>
    ShowStamina = 0x200,
    /// <summary>
    /// Enable oxygen meter in the HUD.
    /// </summary>
    ShowOxygen = 0x400,
    /// <summary>
    /// Enable icons for bleeding, broken bones, temperature, starving, dehydrating, infected, drowning, full moon,
    /// safezone, and arrested status.
    /// </summary>
    ShowStatusIcons = 0x800,
    /// <summary>
    /// Enable UseableGun ammo and firemode in the HUD.
    /// </summary>
    ShowUseableGunStatus = 0x1000,
    /// <summary>
    /// Enable vehicle fuel, speed, health, battery charge, and locked status in the HUD.
    /// </summary>
    ShowVehicleStatus = 0x2000,
    /// <summary>
    /// Enable center dot when guns are not equipped.
    /// </summary>
    ShowCenterDot = 0x4000,
    /// <summary>
    /// Enable popup when in-game rep is increased/decreased.
    /// </summary>
    ShowReputationChangeNotification = 0x8000,
    ShowLifeMeters = 0x7E0,
    /// <summary>
    /// Default flags set when player spawns.
    /// </summary>
    Default = 0xFFF8
}
