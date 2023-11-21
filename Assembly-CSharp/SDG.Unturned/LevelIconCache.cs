using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

/// <summary>
/// Each level should have a 380x80 Icon.png file.
/// This class caches them so that the server list can show them quickly.
/// </summary>
public static class LevelIconCache
{
    private static Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>();

    public static Texture2D GetOrLoadIcon(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        name = name.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        if (icons.TryGetValue(name, out var value))
        {
            return value;
        }
        Texture2D texture2D = LoadIcon(name);
        icons.Add(name, texture2D);
        return texture2D;
    }

    public static Texture2D GetOrLoadIcon(LevelInfo levelInfo)
    {
        if (icons.TryGetValue(levelInfo.name, out var value))
        {
            return value;
        }
        Texture2D texture2D = LoadIcon(levelInfo);
        icons.Add(levelInfo.name, texture2D);
        return texture2D;
    }

    private static Texture2D LoadIcon(string name)
    {
        foreach (CuratedMapLink curated_Map_Link in Provider.statusData.Maps.Curated_Map_Links)
        {
            if (string.Equals(curated_Map_Link.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                string text = PathEx.Join(UnturnedPaths.RootDirectory, "CuratedMapIcons", $"{curated_Map_Link.Workshop_File_Id}.png");
                if (ReadWrite.fileExists(text, useCloud: false, usePath: false))
                {
                    return ReadWrite.readTextureFromFile(text);
                }
            }
        }
        LevelInfo level = Level.getLevel(name);
        if (level != null)
        {
            return LoadIcon(level);
        }
        return null;
    }

    private static Texture2D LoadIcon(LevelInfo levelInfo)
    {
        string text = Path.Combine(levelInfo.path, "Icon.png");
        if (ReadWrite.fileExists(text, useCloud: false, usePath: false))
        {
            return ReadWrite.readTextureFromFile(text);
        }
        return null;
    }
}
