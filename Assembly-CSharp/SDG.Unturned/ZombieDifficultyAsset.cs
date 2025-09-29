using System.Collections.Generic;

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

    /// <summary>
    /// Can horde beacons be placed in the associated bounds?
    /// </summary>
    public bool Allow_Horde_Beacon;

    public EZombieDifficultyHealthOverrideMode SpecialityHealthOverrideMode { get; set; }

    /// <summary>
    /// Can be null if not assigned.
    /// </summary>
    public Dictionary<EZombieSpeciality, float> SpecialityHealthOverrides { get; set; }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        if (p.data.ContainsKey("Overrides_Spawn_Chance"))
        {
            Overrides_Spawn_Chance = p.data.ParseBool("Overrides_Spawn_Chance");
        }
        else
        {
            Overrides_Spawn_Chance = true;
        }
        Crawler_Chance = p.data.ParseFloat("Crawler_Chance");
        Sprinter_Chance = p.data.ParseFloat("Sprinter_Chance");
        Flanker_Chance = p.data.ParseFloat("Flanker_Chance");
        Burner_Chance = p.data.ParseFloat("Burner_Chance");
        Acid_Chance = p.data.ParseFloat("Acid_Chance");
        Boss_Electric_Chance = p.data.ParseFloat("Boss_Electric_Chance");
        Boss_Wind_Chance = p.data.ParseFloat("Boss_Wind_Chance");
        Boss_Fire_Chance = p.data.ParseFloat("Boss_Fire_Chance");
        Spirit_Chance = p.data.ParseFloat("Spirit_Chance");
        DL_Red_Volatile_Chance = p.data.ParseFloat("DL_Red_Volatile_Chance");
        DL_Blue_Volatile_Chance = p.data.ParseFloat("DL_Blue_Volatile_Chance");
        Boss_Elver_Stomper_Chance = p.data.ParseFloat("Boss_Elver_Stomper_Chance");
        Boss_Kuwait_Chance = p.data.ParseFloat("Boss_Kuwait_Chance");
        Mega_Stun_Threshold = p.data.ParseInt32("Mega_Stun_Threshold");
        if (Mega_Stun_Threshold < 1)
        {
            Mega_Stun_Threshold = -1;
        }
        Normal_Stun_Threshold = p.data.ParseInt32("Normal_Stun_Threshold");
        if (Normal_Stun_Threshold < 1)
        {
            Normal_Stun_Threshold = -1;
        }
        if (p.data.ContainsKey("Allow_Horde_Beacon"))
        {
            Allow_Horde_Beacon = p.data.ParseBool("Allow_Horde_Beacon");
        }
        else
        {
            Allow_Horde_Beacon = true;
        }
        SpecialityHealthOverrideMode = p.data.ParseEnum("Speciality_Health_Override_Mode", EZombieDifficultyHealthOverrideMode.None);
        if (SpecialityHealthOverrideMode == EZombieDifficultyHealthOverrideMode.None)
        {
            return;
        }
        if (!p.data.TryGetDictionary("Speciality_Health_Overrides", out var node))
        {
            ReportAssetError("missing Speciality_Health_Overrides");
            return;
        }
        SpecialityHealthOverrides = new Dictionary<EZombieSpeciality, float>();
        for (int i = 1; i <= 24; i++)
        {
            EZombieSpeciality key = (EZombieSpeciality)i;
            string key2 = key.ToString();
            if (node.TryParseFloat(key2, out var value))
            {
                SpecialityHealthOverrides[key] = value;
            }
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
