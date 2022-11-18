using System;
using System.Collections.Generic;
using System.IO;

namespace SDG.Unturned;

public class Localization
{
    private static List<string> _messages;

    private static List<string> keys = new List<string>();

    private static string englishLocalizationRoot = Path.Combine(ReadWrite.PATH, "Localization", "English");

    public static List<string> messages => _messages;

    [Obsolete]
    public static Local tryRead(string path)
    {
        return tryRead(path, usePath: true);
    }

    public static Local tryRead(string path, bool usePath)
    {
        if (usePath)
        {
            path = ReadWrite.PATH + path;
        }
        string path2 = Path.Combine(path, Provider.language + ".dat");
        string path3 = Path.Combine(path, "English.dat");
        if (ReadWrite.fileExists(path2, useCloud: false, usePath: false))
        {
            Data data = ReadWrite.FasterReadDataWithoutHash(path2);
            Data fallbackData = (Provider.languageIsEnglish ? null : ReadWrite.FasterReadDataWithoutHash(path3));
            return new Local(data, fallbackData);
        }
        if (ReadWrite.fileExists(path3, useCloud: false, usePath: false))
        {
            return new Local(ReadWrite.FasterReadDataWithoutHash(path3));
        }
        return new Local();
    }

    public static Local read(string path)
    {
        string path2 = Provider.localizationRoot + path;
        string path3 = englishLocalizationRoot + path;
        if (ReadWrite.fileExists(path2, useCloud: false, usePath: false))
        {
            Data data = ReadWrite.FasterReadDataWithoutHash(path2);
            Data fallbackData = (Provider.languageIsEnglish ? null : ReadWrite.FasterReadDataWithoutHash(path3));
            return new Local(data, fallbackData);
        }
        if (ReadWrite.fileExists(path3, useCloud: false, usePath: false))
        {
            return new Local(ReadWrite.FasterReadDataWithoutHash(path3));
        }
        return new Local();
    }

    private static void scanFile(string path)
    {
        Data data = ReadWrite.FasterReadDataWithoutHash(ReadWrite.PATH + "/Localization/English/" + path);
        Data data2 = ReadWrite.FasterReadDataWithoutHash(Provider.localizationRoot + path);
        KeyValuePair<string, string>[] contents = data.getContents();
        KeyValuePair<string, string>[] contents2 = data2.getContents();
        keys.Clear();
        for (int i = 0; i < contents.Length; i++)
        {
            string key = contents[i].Key;
            bool flag = false;
            for (int j = 0; j < contents2.Length; j++)
            {
                string key2 = contents2[j].Key;
                if (key == key2)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                keys.Add(key);
            }
        }
        if (keys.Count > 0)
        {
            messages.Add(path + " has " + keys.Count + " new keys:");
            for (int k = 0; k < keys.Count; k++)
            {
                messages.Add("[" + k + "]: " + keys[k]);
            }
        }
    }

    private static void scanFolder(string path)
    {
        string[] files = ReadWrite.getFiles("/Localization/English/" + path, usePath: true);
        string[] files2 = ReadWrite.getFiles(Provider.localizationRoot + path, usePath: false);
        for (int i = 0; i < files.Length; i++)
        {
            string fileName = Path.GetFileName(files[i]);
            bool flag = false;
            for (int j = 0; j < files2.Length; j++)
            {
                string fileName2 = Path.GetFileName(files2[j]);
                if (fileName == fileName2)
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                scanFile(path + "/" + fileName);
            }
            else
            {
                messages.Add("New file \"" + fileName + "\" in " + path);
            }
        }
        string[] folders = ReadWrite.getFolders("/Localization/English/" + path, usePath: true);
        string[] folders2 = ReadWrite.getFolders(Provider.localizationRoot + path, usePath: false);
        for (int k = 0; k < folders.Length; k++)
        {
            string fileName3 = Path.GetFileName(folders[k]);
            bool flag2 = false;
            for (int l = 0; l < folders2.Length; l++)
            {
                string fileName4 = Path.GetFileName(folders2[l]);
                if (fileName3 == fileName4)
                {
                    flag2 = true;
                    break;
                }
            }
            if (flag2)
            {
                scanFolder(path + "/" + fileName3);
            }
            else
            {
                messages.Add("New folder \"" + fileName3 + "\" in " + path);
            }
        }
    }

    public static void refresh()
    {
        if (messages == null)
        {
            _messages = new List<string>();
        }
        else
        {
            messages.Clear();
        }
        scanFolder("/Player");
        scanFolder("/Menu");
        scanFolder("/Server");
        scanFolder("/Editor");
    }
}
