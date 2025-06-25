namespace SDG.Unturned;

public enum ERechamberGunAfterReloadMode
{
    /// <summary>
    /// Default. Plays "Hammer" animation if ammo count was zero.
    /// </summary>
    IfAmmoWasEmpty,
    /// <summary>
    /// Regardless of ammo, does not play "Hammer" animation after reloading.
    /// </summary>
    Never,
    /// <summary>
    /// Regardless of ammo, will play "Hammer" animation after reloading.
    /// </summary>
    Always
}
