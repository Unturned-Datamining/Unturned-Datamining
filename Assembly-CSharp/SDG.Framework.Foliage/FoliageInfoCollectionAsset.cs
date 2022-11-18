using System.Collections.Generic;
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage;

public class FoliageInfoCollectionAsset : Asset
{
    public class FoliageInfoCollectionElement : IFormattedFileReadable, IFormattedFileWritable
    {
        public AssetReference<FoliageInfoAsset> asset;

        public float weight;

        public virtual void read(IFormattedFileReader reader)
        {
            reader = reader.readObject();
            if (reader != null)
            {
                asset = reader.readValue<AssetReference<FoliageInfoAsset>>("Asset");
                weight = reader.readValue<float>("Weight");
            }
        }

        public virtual void write(IFormattedFileWriter writer)
        {
            writer.beginObject();
            writer.writeValue("Asset", asset);
            writer.writeValue("Weight", weight);
            writer.endObject();
        }

        public FoliageInfoCollectionElement()
        {
            weight = 1f;
        }
    }

    public List<FoliageInfoCollectionElement> elements;

    public virtual void bakeFoliage(FoliageBakeSettings bakeSettings, IFoliageSurface surface, Bounds bounds, float weight)
    {
        foreach (FoliageInfoCollectionElement element in elements)
        {
            Assets.find(element.asset)?.bakeFoliage(bakeSettings, surface, bounds, weight, element.weight);
        }
    }

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        int num = reader.readArrayLength("Foliage");
        for (int i = 0; i < num; i++)
        {
            elements.Add(reader.readValue<FoliageInfoCollectionElement>(i));
        }
    }

    protected override void writeAsset(IFormattedFileWriter writer)
    {
        base.writeAsset(writer);
        writer.beginArray("Foliage");
        for (int i = 0; i < elements.Count; i++)
        {
            writer.writeValue(elements[i]);
        }
        writer.endArray();
    }

    public FoliageInfoCollectionAsset()
    {
        elements = new List<FoliageInfoCollectionElement>();
    }

    public FoliageInfoCollectionAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
        elements = new List<FoliageInfoCollectionElement>();
    }
}
