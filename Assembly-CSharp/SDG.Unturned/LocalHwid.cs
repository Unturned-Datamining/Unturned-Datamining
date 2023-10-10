using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SDG.Unturned;

public static class LocalHwid
{
    internal const byte MAX_HWIDS = 8;

    private const string SALT = "Zpsz+h>nJ!?4h2&nVPVw=DmG";

    public static byte[][] GetHwids()
    {
        byte[][] array = InitHwids();
        if (array == null)
        {
            array = new byte[1][] { CreateRandomHwid() };
        }
        return array;
    }

    private static byte[] CreateRandomHwid()
    {
        byte[] array = new byte[20];
        for (int i = 0; i < 20; i++)
        {
            array[i] = (byte)UnityEngine.Random.Range(0, 256);
        }
        return array;
    }

    private static byte[][] InitHwids()
    {
        List<byte[]> list = GatherAvailableHwids();
        if (list == null || list.Count < 1)
        {
            return null;
        }
        if (list.Count > 8)
        {
            byte[][] array = new byte[8][];
            for (int i = 0; i < 8; i++)
            {
                int randomIndex = list.GetRandomIndex();
                array[i] = list[randomIndex];
                list.RemoveAtFast(randomIndex);
            }
            return array;
        }
        return list.ToArray();
    }

    private static void GatherPlayerPrefsEntry(List<byte[]> results)
    {
        byte[] array = new byte[16]
        {
            104, 115, 97, 72, 101, 103, 97, 114, 111, 116,
            83, 100, 117, 111, 108, 67
        };
        Array.Reverse(array);
        string @string = Encoding.UTF8.GetString(array);
        string text = PlayerPrefs.GetString(@string);
        if (string.IsNullOrEmpty(text) || text.Length != 32)
        {
            text = Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(@string, text);
            PlayerPrefs.Save();
        }
        results.Add(Hash.SHA1("Zpsz+h>nJ!?4h2&nVPVw=DmG" + text));
    }

    private static void GatherConvenientSavedataEntry(List<byte[]> results)
    {
        byte[] array = new byte[14]
        {
            101, 104, 99, 97, 67, 101, 114, 111, 116, 83,
            109, 101, 116, 73
        };
        Array.Reverse(array);
        string @string = Encoding.UTF8.GetString(array);
        if (!ConvenientSavedata.get().read(@string, out string value) || value.Length != 32)
        {
            value = Guid.NewGuid().ToString("N");
            ConvenientSavedata.get().write(@string, value);
        }
        results.Add(Hash.SHA1("Zpsz+h>nJ!?4h2&nVPVw=DmG" + value));
    }

    private static List<byte[]> GatherAvailableHwids()
    {
        List<byte[]> list = new List<byte[]>();
        GatherPlayerPrefsEntry(list);
        GatherConvenientSavedataEntry(list);
        return list;
    }
}
