namespace SDG.Unturned;

public class ZombieDifficultyAsset : Asset
{
    public bool Overrides_Spawn_Chance;

    public float Crawler_Chance;

    public float Sprinter_Chance;

    public float Flanker_Chance;

    public float Burner_Chance;

    public float Acid_Chance;

    public float Boss_Electric_Chance;

    public float Boss_Wind_Chance;

    public float Boss_Fire_Chance;

    public float Spirit_Chance;

    public float DL_Red_Volatile_Chance;

    public float DL_Blue_Volatile_Chance;

    public float Boss_Elver_Stomper_Chance;

    public float Boss_Kuwait_Chance;

    public int Mega_Stun_Threshold;

    public int Normal_Stun_Threshold;

    public bool Allow_Horde_Beacon;

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        if (data.ContainsKey("Overrides_Spawn_Chance"))
        {
            Overrides_Spawn_Chance = data.ParseBool("Overrides_Spawn_Chance");
        }
        else
        {
            Overrides_Spawn_Chance = true;
        }
        Crawler_Chance = data.ParseFloat("Crawler_Chance");
        Sprinter_Chance = data.ParseFloat("Sprinter_Chance");
        Flanker_Chance = data.ParseFloat("Flanker_Chance");
        Burner_Chance = data.ParseFloat("Burner_Chance");
        Acid_Chance = data.ParseFloat("Acid_Chance");
        Boss_Electric_Chance = data.ParseFloat("Boss_Electric_Chance");
        Boss_Wind_Chance = data.ParseFloat("Boss_Wind_Chance");
        Boss_Fire_Chance = data.ParseFloat("Boss_Fire_Chance");
        Spirit_Chance = data.ParseFloat("Spirit_Chance");
        DL_Red_Volatile_Chance = data.ParseFloat("DL_Red_Volatile_Chance");
        DL_Blue_Volatile_Chance = data.ParseFloat("DL_Blue_Volatile_Chance");
        Boss_Elver_Stomper_Chance = data.ParseFloat("Boss_Elver_Stomper_Chance");
        Boss_Kuwait_Chance = data.ParseFloat("Boss_Kuwait_Chance");
        Mega_Stun_Threshold = data.ParseInt32("Mega_Stun_Threshold");
        if (Mega_Stun_Threshold < 1)
        {
            Mega_Stun_Threshold = -1;
        }
        Normal_Stun_Threshold = data.ParseInt32("Normal_Stun_Threshold");
        if (Normal_Stun_Threshold < 1)
        {
            Normal_Stun_Threshold = -1;
        }
        if (data.ContainsKey("Allow_Horde_Beacon"))
        {
            Allow_Horde_Beacon = data.ParseBool("Allow_Horde_Beacon");
        }
        else
        {
            Allow_Horde_Beacon = true;
        }
    }

    protected virtual void construct()
    {
        Allow_Horde_Beacon = true;
    }

    public ZombieDifficultyAsset()
    {
        construct();
    }
}
