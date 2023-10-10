using SDG.Framework.IO.FormattedFiles;

namespace SDG.Unturned;

public class HordePurchaseVolume : LevelVolume<HordePurchaseVolume, HordePurchaseVolumeManager>
{
    private class Menu : SleekWrapper
    {
        private HordePurchaseVolume volume;

        public Menu(HordePurchaseVolume volume)
        {
            this.volume = volume;
            base.SizeOffset_X = 400f;
            base.SizeOffset_Y = 70f;
            ISleekUInt16Field sleekUInt16Field = Glazier.Get().CreateUInt16Field();
            sleekUInt16Field.SizeOffset_X = 200f;
            sleekUInt16Field.SizeOffset_Y = 30f;
            sleekUInt16Field.Value = volume.id;
            sleekUInt16Field.AddLabel("Item ID", ESleekSide.RIGHT);
            sleekUInt16Field.OnValueChanged += OnIdChanged;
            AddChild(sleekUInt16Field);
            ISleekUInt32Field sleekUInt32Field = Glazier.Get().CreateUInt32Field();
            sleekUInt32Field.PositionOffset_Y = 40f;
            sleekUInt32Field.SizeOffset_X = 200f;
            sleekUInt32Field.SizeOffset_Y = 30f;
            sleekUInt32Field.Value = volume.cost;
            sleekUInt32Field.AddLabel("Cost", ESleekSide.RIGHT);
            sleekUInt32Field.OnValueChanged += OnCostChanged;
            AddChild(sleekUInt32Field);
        }

        private void OnIdChanged(ISleekUInt16Field field, ushort state)
        {
            volume.id = state;
        }

        private void OnCostChanged(ISleekUInt32Field field, uint state)
        {
            volume.cost = state;
        }
    }

    public ushort id;

    public uint cost;

    public override ISleekElement CreateMenu()
    {
        ISleekElement sleekElement = new Menu(this);
        AppendBaseMenu(sleekElement);
        return sleekElement;
    }

    protected override void readHierarchyItem(IFormattedFileReader reader)
    {
        base.readHierarchyItem(reader);
        if (reader.containsKey("Item_ID"))
        {
            id = reader.readValue<ushort>("Item_ID");
        }
        if (reader.containsKey("Cost"))
        {
            cost = reader.readValue<uint>("Cost");
        }
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("Item_ID", id);
        writer.writeValue("Cost", cost);
    }
}
