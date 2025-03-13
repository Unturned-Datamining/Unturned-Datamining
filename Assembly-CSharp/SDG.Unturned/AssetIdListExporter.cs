using System;
using System.Collections.Generic;
using System.IO;
using Unturned.SystemEx;

namespace SDG.Unturned;

public static class AssetIdListExporter
{
    public static void Export()
    {
        List<Asset> list = new List<Asset>();
        Assets.FindAssetsByType_UseDefaultAssetMapping(list);
        string text = Path.Join(ReadWrite.PATH, "Extras", "AssetIDs");
        ReadWrite.createFolder(text, usePath: false);
        string text2 = Path.Join(text, "All Assets");
        ReadWrite.createFolder(text2, usePath: false);
        ExportAssetsToCsv(list, Path.Join(text2, "All Assets.csv"));
        ExportAssetsToCsvGroupedByLegacyCategory(text2, list);
        ExportAssetsToCsvGroupedByType(text2, list);
        foreach (AssetOrigin assetOrigin in Assets.assetOrigins)
        {
            if (!assetOrigin.assets.IsEmpty())
            {
                string text3 = PathEx.ReplaceInvalidFileNameChars(assetOrigin.name, '_');
                if (string.IsNullOrEmpty(text3))
                {
                    UnturnedLog.error("Unable to export origin " + assetOrigin.name + " Asset IDs because file name would be empty");
                    continue;
                }
                string text4 = Path.Join(text, text3);
                ReadWrite.createFolder(text4, usePath: false);
                ExportAssetsToCsv(csvPath: Path.Join(text4, text3 + ".csv"), assets: assetOrigin.assets);
                ExportAssetsToCsvGroupedByLegacyCategory(text4, assetOrigin.assets);
                ExportAssetsToCsvGroupedByType(text4, assetOrigin.assets);
            }
        }
    }

    private static void ExportAssetsToCsvGroupedByLegacyCategory(string basePath, List<Asset> assets)
    {
        basePath = Path.Join(basePath, "Grouped by Legacy Category");
        ReadWrite.createFolder(basePath, usePath: false);
        foreach (KeyValuePair<EAssetType, List<Asset>> item in GroupAssetsByLegacyCategory(assets))
        {
            string text = item.Key.ToString();
            string csvPath = Path.Combine(basePath, text + ".csv");
            ExportAssetsToCsv(item.Value, csvPath);
            string csvPath2 = Path.Combine(basePath, text + " Legacy ID Availability.csv");
            ExportLegacyIdAvailabilityToCsv(item.Key, item.Value, csvPath2);
        }
    }

    private static void ExportAssetsToCsvGroupedByType(string basePath, List<Asset> assets)
    {
        basePath = Path.Join(basePath, "Grouped by Type");
        ReadWrite.createFolder(basePath, usePath: false);
        foreach (KeyValuePair<Type, List<Asset>> item in GroupAssetsByType(assets))
        {
            string typeFriendlyName = item.Value[0].GetTypeFriendlyName();
            string csvPath = Path.Combine(basePath, typeFriendlyName + ".csv");
            ExportAssetsToCsv(item.Value, csvPath);
        }
    }

    private static Dictionary<EAssetType, List<Asset>> GroupAssetsByLegacyCategory(List<Asset> assets)
    {
        Dictionary<EAssetType, List<Asset>> dictionary = new Dictionary<EAssetType, List<Asset>>();
        foreach (Asset asset in assets)
        {
            EAssetType assetCategory = asset.assetCategory;
            if (!dictionary.TryGetValue(assetCategory, out var value))
            {
                value = (dictionary[assetCategory] = new List<Asset>());
            }
            value.Add(asset);
        }
        return dictionary;
    }

    private static Dictionary<Type, List<Asset>> GroupAssetsByType(List<Asset> assets)
    {
        Dictionary<Type, List<Asset>> dictionary = new Dictionary<Type, List<Asset>>();
        foreach (Asset asset in assets)
        {
            Type type = asset.GetType();
            if (!dictionary.TryGetValue(type, out var value))
            {
                value = (dictionary[type] = new List<Asset>());
            }
            value.Add(asset);
        }
        return dictionary;
    }

    private static void ExportAssetsToCsv(IEnumerable<Asset> assets, string csvPath)
    {
        using FileStream stream = new FileStream(csvPath, FileMode.Create, FileAccess.Write);
        using StreamWriter streamWriter = new StreamWriter(stream);
        streamWriter.WriteLine("Name,GUID,Type,Origin,Legacy ID,Legacy Category");
        foreach (Asset asset in assets)
        {
            WriteEscapedString(streamWriter, asset.FriendlyName);
            streamWriter.Write(',');
            streamWriter.Write(asset.GUID.ToString("N"));
            streamWriter.Write(',');
            streamWriter.Write(asset.GetTypeFriendlyName());
            streamWriter.Write(',');
            WriteEscapedString(streamWriter, asset.GetOriginName());
            streamWriter.Write(',');
            streamWriter.Write(asset.id);
            streamWriter.Write(',');
            streamWriter.WriteLine(asset.assetCategory.ToString());
        }
    }

    private static void ExportLegacyIdAvailabilityToCsv(EAssetType legacyType, IEnumerable<Asset> assets, string csvPath)
    {
        Dictionary<int, List<Asset>> dictionary = new Dictionary<int, List<Asset>>();
        foreach (Asset asset in assets)
        {
            if (dictionary.TryGetValue(asset.id, out var value))
            {
                value.Add(asset);
                continue;
            }
            value = new List<Asset> { asset };
            dictionary.Add(asset.id, value);
        }
        using FileStream stream = new FileStream(csvPath, FileMode.Create, FileAccess.Write);
        using StreamWriter streamWriter = new StreamWriter(stream);
        streamWriter.WriteLine("Legacy ID,Used By,Reserved for Vanilla");
        int num = 0;
        switch (legacyType)
        {
        case EAssetType.ITEM:
            num = 2000;
            break;
        case EAssetType.EFFECT:
            num = 200;
            break;
        case EAssetType.RESOURCE:
            num = 50;
            break;
        case EAssetType.ANIMAL:
            num = 50;
            break;
        case EAssetType.MYTHIC:
            num = 500;
            break;
        case EAssetType.SKIN:
            num = 2000;
            break;
        case EAssetType.NPC:
            num = 2000;
            break;
        }
        for (int i = 1; i <= 65535; streamWriter.WriteLine(), i++)
        {
            streamWriter.Write(i);
            streamWriter.Write(',');
            if (dictionary.TryGetValue(i, out var value2))
            {
                string text = value2[0].FriendlyName;
                for (int j = 1; j < value2.Count; j++)
                {
                    text += ", ";
                    text += value2[j].FriendlyName;
                }
                WriteEscapedString(streamWriter, text);
            }
            else
            {
                streamWriter.Write("---");
            }
            streamWriter.Write(',');
            if (i >= num)
            {
                continue;
            }
            if (legacyType == EAssetType.ITEM)
            {
                switch (i)
                {
                case 786:
                case 1800:
                case 1801:
                case 1802:
                case 1803:
                case 1804:
                case 1805:
                case 1806:
                    streamWriter.Write("Hawaii Overlap");
                    continue;
                }
            }
            streamWriter.Write("Reserved");
        }
    }

    private static void WriteEscapedString(StreamWriter sw, string value)
    {
        sw.Write('"');
        foreach (char c in value)
        {
            if (c == '"')
            {
                sw.Write('"');
            }
            sw.Write(c);
        }
        sw.Write('"');
    }
}
