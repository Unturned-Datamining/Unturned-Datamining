namespace SDG.Unturned;

public static class ZombieSpecialityExtension
{
    public static bool IsDLVolatile(this EZombieSpeciality speciality)
    {
        return speciality == EZombieSpeciality.DL_RED_VOLATILE || speciality == EZombieSpeciality.DL_BLUE_VOLATILE;
    }

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
            return true;
        default:
            return false;
        }
    }
}
