using SDG.Framework.Devkit;
using SDG.Framework.IO.FormattedFiles;

namespace SDG.Unturned;

public class SafezoneVolume : LevelVolume<SafezoneVolume, SafezoneVolumeManager>
{
    private class Menu : SleekWrapper
    {
        private SafezoneVolume volume;

        public Menu(SafezoneVolume volume)
        {
            this.volume = volume;
            base.SizeOffset_X = 400f;
            base.SizeOffset_Y = 140f;
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.SizeOffset_X = 40f;
            sleekToggle.SizeOffset_Y = 40f;
            sleekToggle.Value = volume.noWeapons;
            sleekToggle.AddLabel("No Weapons", ESleekSide.RIGHT);
            sleekToggle.OnValueChanged += OnWeaponsToggled;
            AddChild(sleekToggle);
            ISleekToggle sleekToggle2 = Glazier.Get().CreateToggle();
            sleekToggle2.PositionOffset_Y = 50f;
            sleekToggle2.SizeOffset_X = 40f;
            sleekToggle2.SizeOffset_Y = 40f;
            sleekToggle2.Value = volume.noBuildables;
            sleekToggle2.AddLabel("No Buildables", ESleekSide.RIGHT);
            sleekToggle2.OnValueChanged += OnBuildablesToggled;
            AddChild(sleekToggle2);
            ISleekToggle sleekToggle3 = Glazier.Get().CreateToggle();
            sleekToggle3.PositionOffset_Y = 100f;
            sleekToggle3.SizeOffset_X = 40f;
            sleekToggle3.SizeOffset_Y = 40f;
            sleekToggle3.Value = volume.noIncomingDamage;
            sleekToggle3.AddLabel("No Incoming Damage", ESleekSide.RIGHT);
            sleekToggle3.OnValueChanged += OnIncomingDamageToggled;
            AddChild(sleekToggle3);
        }

        private void OnWeaponsToggled(ISleekToggle toggle, bool state)
        {
            volume.noWeapons = state;
            LevelHierarchy.MarkDirty();
        }

        private void OnBuildablesToggled(ISleekToggle toggle, bool state)
        {
            volume.noBuildables = state;
            LevelHierarchy.MarkDirty();
        }

        private void OnIncomingDamageToggled(ISleekToggle toggle, bool state)
        {
            volume.noIncomingDamage = state;
            LevelHierarchy.MarkDirty();
        }
    }

    /// <summary>
    /// If true, players inside the safezone cannot use items categorized as "weapons" (/hostile).
    /// </summary>
    public bool noWeapons = true;

    public bool noBuildables = true;

    /// <summary>
    /// If true, players inside the safezone cannot take damage. (Unless damage's bypassSafezone parameter is true.)
    /// For backwards compatibility this is true if noWeapons was true.
    /// </summary>
    public bool noIncomingDamage = true;

    internal SafezoneNode backwardsCompatibilityNode;

    public override ISleekElement CreateMenu()
    {
        ISleekElement sleekElement = new Menu(this);
        AppendBaseMenu(sleekElement);
        return sleekElement;
    }

    protected override void readHierarchyItem(IFormattedFileReader reader)
    {
        base.readHierarchyItem(reader);
        if (reader.containsKey("No_Weapons"))
        {
            noWeapons = reader.readValue<bool>("No_Weapons");
        }
        if (reader.containsKey("No_Buildables"))
        {
            noBuildables = reader.readValue<bool>("No_Buildables");
        }
        if (reader.containsKey("No_Incoming_Damage"))
        {
            noIncomingDamage = reader.readValue<bool>("No_Incoming_Damage");
        }
        else
        {
            noIncomingDamage = noWeapons;
        }
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("No_Weapons", noWeapons);
        writer.writeValue("No_Buildables", noBuildables);
        writer.writeValue("No_Incoming_Damage", noIncomingDamage);
    }

    protected override void Start()
    {
        base.Start();
        backwardsCompatibilityNode = new SafezoneNode(base.transform.position, SafezoneNode.CalculateNormalizedRadiusFromRadius(GetSphereRadius()), newHeight: false, noWeapons, noBuildables);
        backwardsCompatibilityNode.noIncomingDamage = noIncomingDamage;
    }
}
