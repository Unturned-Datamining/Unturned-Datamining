using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Steamworks;

namespace SDG.Unturned;

public class Hash
{
    private static SHA1CryptoServiceProvider service = new SHA1CryptoServiceProvider();

    private static byte[] _40bytes = new byte[40];

    public static byte[] SHA1(byte[] bytes)
    {
        return service.ComputeHash(bytes);
    }

    public static byte[] SHA1(Stream stream)
    {
        return service.ComputeHash(stream);
    }

    public static byte[] SHA1(string text)
    {
        return SHA1(Encoding.UTF8.GetBytes(text));
    }

    public static byte[] SHA1(CSteamID steamID)
    {
        return SHA1(BitConverter.GetBytes(steamID.m_SteamID));
    }

    public static bool verifyHash(byte[] hash_0, byte[] hash_1)
    {
        if (hash_0.Length != 20 || hash_1.Length != 20)
        {
            return false;
        }
        for (int i = 0; i < hash_0.Length; i++)
        {
            if (hash_0[i] != hash_1[i])
            {
                return false;
            }
        }
        return true;
    }

    public static byte[] combineSHA1Hashes(byte[] a, byte[] b)
    {
        if (a.Length != 20 || b.Length != 20)
        {
            throw new Exception("both lengths should be 20");
        }
        a.CopyTo(_40bytes, 0);
        b.CopyTo(_40bytes, 20);
        return SHA1(_40bytes);
    }

    public static byte[] combine(params byte[][] hashes)
    {
        byte[] array = new byte[hashes.Length * 20];
        for (int i = 0; i < hashes.Length; i++)
        {
            hashes[i].CopyTo(array, i * 20);
        }
        return SHA1(array);
    }

    public static byte[] combine(List<byte[]> hashes)
    {
        byte[] array = new byte[hashes.Count * 20];
        for (int i = 0; i < hashes.Count; i++)
        {
            hashes[i].CopyTo(array, i * 20);
        }
        return SHA1(array);
    }

    public static string toString(byte[] hash)
    {
        if (hash == null)
        {
            return "null";
        }
        string text = "";
        for (int i = 0; i < hash.Length; i++)
        {
            text += hash[i].ToString("X2");
        }
        return text;
    }

    internal static string ToCodeString(byte[] hash)
    {
        StringBuilder stringBuilder = new StringBuilder(hash.Length * 6);
        stringBuilder.Append("0x");
        stringBuilder.Append(hash[0].ToString("X2"));
        for (int i = 1; i < hash.Length; i++)
        {
            stringBuilder.Append(", 0x");
            stringBuilder.Append(hash[i].ToString("X2"));
        }
        return stringBuilder.ToString();
    }

    public static void log(byte[] hash)
    {
        if (hash != null && hash.Length == 20)
        {
            CommandWindow.Log(toString(hash));
        }
    }
}
