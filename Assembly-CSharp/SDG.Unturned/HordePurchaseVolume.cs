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
            base.sizeOffset_X = 400;
            base.sizeOffset_Y = 70;
            ISleekUInt16Field sleekUInt16Field = Glazier.Get().CreateUInt16Field();
            sleekUInt16Field.sizeOffset_X = 200;
            sleekUInt16Field.sizeOffset_Y = 30;
            sleekUInt16Field.state = volume.id;
            sleekUInt16Field.addLabel("Item ID", ESleekSide.RIGHT);
            sleekUInt16Field.onTypedUInt16 += OnIdChanged;
            AddChild(sleekUInt16Field);
            ISleekUInt32Field sleekUInt32Field = Glazier.Get().CreateUInt32Field();
            sleekUInt32Field.positionOffset_Y = 40;
            sleekUInt32Field.sizeOffset_X = 200;
            sleekUInt32Field.sizeOffset_Y = 30;
            sleekUInt32Field.state = volume.cost;
            sleekUInt32Field.addLabel("Cost", ESleekSide.RIGHT);
            sleekUInt32Field.onTypedUInt32 += OnCostChanged;
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
