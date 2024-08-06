using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Utility for getting local hardware ID.
///
/// One option for future improvement would be using Windows Management Infrastructure (WMI) API:
/// https://github.com/SmartlyDressedGames/Unturned-3.x/issues/1593
/// </summary>
public static class LocalHwid
{
    /// <summary>
    /// Maximum number of HWIDs before server will reject connection request.
    /// </summary>
    internal const byte MAX_HWIDS = 8;

    private const string SALT = "Zpsz+h>nJ!?4h2&nVPVw=DmG";

    /// <summary>
    /// Get the local hardware ID(s).
    /// </summary>
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
        bool flag = false;
        if (string.IsNullOrEmpty(text) || text.Length != 32)
        {
            text = Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(@string, text);
            flag = true;
        }
        byte b = 240;
        string text2 = text;
        foreach (char c in text2)
        {
            b = (byte)(b * 3 + c);
        }
        if (PlayerPrefs.HasKey("unity.player_session_restoreflags"))
        {
            if (PlayerPrefs.GetInt("unity.player_session_restoreflags") != b)
            {
                Provider.catPouncingMechanism = UnityEngine.Random.Range(19.83f, 151.25f);
            }
        }
        else
        {
            PlayerPrefs.SetInt("unity.player_session_restoreflags", b);
            flag = true;
        }
        if (flag)
        {
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
        byte b = 240;
        string text = value;
        foreach (char c in text)
        {
            b = (byte)(b * 3 + c);
        }
        if (ConvenientSavedata.get().read("NewItemSeenPromotionId", out long value2))
        {
            if (value2 != b)
            {
                Provider.catPouncingMechanism = UnityEngine.Random.Range(19.83f, 151.25f);
            }
        }
        else
        {
            ConvenientSavedata.get().write("NewItemSeenPromotionId", b);
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
