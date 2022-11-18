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
            base.sizeOffset_X = 400;
            base.sizeOffset_Y = 90;
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.sizeOffset_X = 40;
            sleekToggle.sizeOffset_Y = 40;
            sleekToggle.state = volume.noWeapons;
            sleekToggle.addLabel("No Weapons", ESleekSide.RIGHT);
            sleekToggle.onToggled += OnWeaponsToggled;
            AddChild(sleekToggle);
            ISleekToggle sleekToggle2 = Glazier.Get().CreateToggle();
            sleekToggle2.positionOffset_Y = 50;
            sleekToggle2.sizeOffset_X = 40;
            sleekToggle2.sizeOffset_Y = 40;
            sleekToggle2.state = volume.noBuildables;
            sleekToggle2.addLabel("No Buildables", ESleekSide.RIGHT);
            sleekToggle2.onToggled += OnBuildablesToggled;
            AddChild(sleekToggle2);
        }

        private void OnWeaponsToggled(ISleekToggle toggle, bool state)
        {
            volume.noWeapons = state;
        }

        private void OnBuildablesToggled(ISleekToggle toggle, bool state)
        {
            volume.noBuildables = state;
        }
    }

    public bool noWeapons = true;

    public bool noBuildables = true;

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
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("No_Weapons", noWeapons);
        writer.writeValue("No_Buildables", noBuildables);
    }

    protected override void Start()
    {
        base.Start();
        backwardsCompatibilityNode = new SafezoneNode(base.transform.position, SafezoneNode.CalculateNormalizedRadiusFromRadius(GetSphereRadius()), newHeight: false, noWeapons, noBuildables);
    }
}
