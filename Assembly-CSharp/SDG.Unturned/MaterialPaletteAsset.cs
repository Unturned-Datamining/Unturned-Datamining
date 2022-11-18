using SDG.Framework.Debug;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned;

public class MaterialPaletteAsset : Asset
{
    public InspectableList<ContentReference<Material>> materials;

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        int num = reader.readArrayLength("Materials");
        for (int i = 0; i < num; i++)
        {
            materials.Add(reader.readValue<ContentReference<Material>>(i));
        }
    }

    protected override void writeAsset(IFormattedFileWriter writer)
    {
        base.writeAsset(writer);
        writer.beginArray("Materials");
        for (int i = 0; i < materials.Count; i++)
        {
            writer.writeValue(materials[i]);
        }
        writer.endArray();
    }

    public MaterialPaletteAsset()
    {
        materials = new InspectableList<ContentReference<Material>>();
    }

    public MaterialPaletteAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
        materials = new InspectableList<ContentReference<Material>>();
    }
}
