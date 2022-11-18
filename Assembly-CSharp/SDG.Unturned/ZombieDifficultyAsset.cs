using SDG.Framework.IO.FormattedFiles;

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

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        if (reader.containsKey("Overrides_Spawn_Chance"))
        {
            Overrides_Spawn_Chance = reader.readValue<bool>("Overrides_Spawn_Chance");
        }
        else
        {
            Overrides_Spawn_Chance = true;
        }
        Crawler_Chance = reader.readValue<float>("Crawler_Chance");
        Sprinter_Chance = reader.readValue<float>("Sprinter_Chance");
        Flanker_Chance = reader.readValue<float>("Flanker_Chance");
        Burner_Chance = reader.readValue<float>("Burner_Chance");
        Acid_Chance = reader.readValue<float>("Acid_Chance");
        Boss_Electric_Chance = reader.readValue<float>("Boss_Electric_Chance");
        Boss_Wind_Chance = reader.readValue<float>("Boss_Wind_Chance");
        Boss_Fire_Chance = reader.readValue<float>("Boss_Fire_Chance");
        Spirit_Chance = reader.readValue<float>("Spirit_Chance");
        DL_Red_Volatile_Chance = reader.readValue<float>("DL_Red_Volatile_Chance");
        DL_Blue_Volatile_Chance = reader.readValue<float>("DL_Blue_Volatile_Chance");
        Boss_Elver_Stomper_Chance = reader.readValue<float>("Boss_Elver_Stomper_Chance");
        Boss_Kuwait_Chance = reader.readValue<float>("Boss_Kuwait_Chance");
        Mega_Stun_Threshold = reader.readValue<int>("Mega_Stun_Threshold");
        if (Mega_Stun_Threshold < 1)
        {
            Mega_Stun_Threshold = -1;
        }
        Normal_Stun_Threshold = reader.readValue<int>("Normal_Stun_Threshold");
        if (Normal_Stun_Threshold < 1)
        {
            Normal_Stun_Threshold = -1;
        }
        if (reader.containsKey("Allow_Horde_Beacon"))
        {
            Allow_Horde_Beacon = reader.readValue<bool>("Allow_Horde_Beacon");
        }
        else
        {
            Allow_Horde_Beacon = true;
        }
    }

    protected override void writeAsset(IFormattedFileWriter writer)
    {
        base.writeAsset(writer);
        writer.writeValue("Overrides_Spawn_Chance", Overrides_Spawn_Chance);
        writer.writeValue("Crawler_Chance", Crawler_Chance);
        writer.writeValue("Sprinter_Chance", Sprinter_Chance);
        writer.writeValue("Flanker_Chance", Flanker_Chance);
        writer.writeValue("Burner_Chance", Burner_Chance);
        writer.writeValue("Acid_Chance", Acid_Chance);
        writer.writeValue("Boss_Electric_Chance", Boss_Electric_Chance);
        writer.writeValue("Boss_Wind_Chance", Boss_Wind_Chance);
        writer.writeValue("Boss_Fire_Chance", Boss_Fire_Chance);
        writer.writeValue("Spirit_Chance", Spirit_Chance);
        writer.writeValue("DL_Red_Volatile_Chance", DL_Red_Volatile_Chance);
        writer.writeValue("DL_Blue_Volatile_Chance", DL_Blue_Volatile_Chance);
        writer.writeValue("Boss_Elver_Stomper_Chance", Boss_Elver_Stomper_Chance);
        writer.writeValue("Boss_Kuwait_Chance", Boss_Kuwait_Chance);
        writer.writeValue("Mega_Stun_Threshold", Mega_Stun_Threshold);
        writer.writeValue("Normal_Stun_Threshold", Normal_Stun_Threshold);
        writer.writeValue("Allow_Horde_Beacon", Allow_Horde_Beacon);
    }

    protected virtual void construct()
    {
        Allow_Horde_Beacon = true;
    }

    public ZombieDifficultyAsset()
    {
        construct();
    }

    public ZombieDifficultyAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
        construct();
    }
}
