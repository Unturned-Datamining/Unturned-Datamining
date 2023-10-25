using System;
using System.Collections.Generic;
using System.IO;

namespace SDG.Unturned;

/// <summary>
/// Allows mappers to bulk replace assets by listing pairs in a text file.
/// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/2275
/// </summary>
public static class EditorAssetRedirector
{
    private static Dictionary<Guid, Guid> mappings;

    public static bool HasRedirects
    {
        get
        {
            if (mappings != null)
            {
                return mappings.Count > 0;
            }
            return false;
        }
    }

    public static ObjectAsset RedirectObject(Guid oldGuid)
    {
        if (mappings.TryGetValue(oldGuid, out var value))
        {
            return Assets.find(value) as ObjectAsset;
        }
        return null;
    }

    static EditorAssetRedirector()
    {
        string path = Path.Combine(ReadWrite.PATH, "EditorAssetRedirectors.txt");
        if (!File.Exists(path))
        {
            return;
        }
        mappings = new Dictionary<Guid, Guid>();
        string[] array = File.ReadAllLines(path);
        for (int i = 0; i < array.Length; i++)
        {
            string text = array[i];
            if (string.IsNullOrWhiteSpace(text) || text.StartsWith("#") || text.StartsWith("//"))
            {
                continue;
            }
            int num = text.IndexOf("->");
            if (num < 0 || num + 2 >= text.Length)
            {
                UnturnedLog.warn("Unable to split \"->\" in editor asset redirect \"{0}\" (line {1})", text, i + 1);
                continue;
            }
            string text2 = text.Substring(0, num).Trim();
            string text3 = text.Substring(num + 2).Trim();
            Guid result2;
            Guid value;
            if (!Guid.TryParse(text2, out var result))
            {
                UnturnedLog.warn("Unable to parse \"{0}\" as old guid from \"{1}\" (line {2})", text2, text, i + 1);
            }
            else if (!Guid.TryParse(text3, out result2))
            {
                UnturnedLog.warn("Unable to parse \"{0}\" as new guid from \"{1}\" (line {2})", text3, text, i + 1);
            }
            else if (mappings.TryGetValue(result, out value))
            {
                UnturnedLog.warn("Editor asset redirect {0} to {1} (line {2}) conflicts with prior redirect to {3}", result, result2, i + 1, value);
            }
            else
            {
                mappings.Add(result, result2);
                UnturnedLog.info("Editor redirecting asset {0} to {1}", result, result2);
            }
        }
    }
}
