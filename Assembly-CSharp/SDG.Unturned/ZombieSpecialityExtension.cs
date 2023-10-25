namespace SDG.Unturned;

public static class ZombieSpecialityExtension
{
    /// <summary>
    /// Is this one of the Dying Light volatile zombies? Only spawns during night. Explodes into fire at dawn.
    /// </summary>
    public static bool IsDLVolatile(this EZombieSpeciality speciality)
    {
        return speciality == EZombieSpeciality.DL_RED_VOLATILE || speciality == EZombieSpeciality.DL_BLUE_VOLATILE;
    }

    /// <summary>
    /// Does this have the BOSS_* prefix?
    /// </summary>
    public static bool IsBoss(this EZombieSpeciality speciality)
    {
        switch (speciality)
        {
        case EZombieSpeciality.BOSS_ELECTRIC:
        case EZombieSpeciality.BOSS_WIND:
        case EZombieSpeciality.BOSS_FIRE:
        case EZombieSpeciality.BOSS_MAGMA:
        case EZombieSpeciality.BOSS_SPIRIT:
        case EZombieSpeciality.BOSS_NUCLEAR:
        case EZombieSpeciality.BOSS_ELVER_STOMPER:
        case EZombieSpeciality.BOSS_KUWAIT:
        case EZombieSpeciality.BOSS_BUAK_ELECTRIC:
        case EZombieSpeciality.BOSS_BUAK_WIND:
        case EZombieSpeciality.BOSS_BUAK_FIRE:
        case EZombieSpeciality.BOSS_BUAK_FINAL:
            return true;
        default:
            return false;
        }
    }

    public static bool IsFromBuakMap(this EZombieSpeciality speciality)
    {
        if ((uint)(speciality - 21) <= 3u)
        {
            return true;
        }
        return false;
    }
}
