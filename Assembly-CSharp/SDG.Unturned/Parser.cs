using System.Collections.Generic;

namespace SDG.Unturned;

public class Parser
{
    public static bool trySplitStart(string serial, out string start, out string end)
    {
        start = "";
        end = "";
        int num = serial.IndexOf(' ');
        if (num == -1)
        {
            return false;
        }
        start = serial.Substring(0, num);
        end = serial.Substring(num + 1, serial.Length - num - 1);
        return true;
    }

    public static bool trySplitEnd(string serial, out string start, out string end)
    {
        start = "";
        end = "";
        int num = serial.LastIndexOf(' ');
        if (num == -1)
        {
            return false;
        }
        start = serial.Substring(0, num);
        end = serial.Substring(num + 1, serial.Length - num - 1);
        return true;
    }

    public static string[] getComponentsFromSerial(string serial, char delimiter)
    {
        List<string> list = new List<string>();
        int num = 0;
        while (num < serial.Length)
        {
            int num2 = serial.IndexOf(delimiter, num);
            if (num2 == -1)
            {
                list.Add(serial.Substring(num, serial.Length - num));
                break;
            }
            list.Add(serial.Substring(num, num2 - num));
            num = num2 + 1;
        }
        return list.ToArray();
    }

    public static string getSerialFromComponents(char delimiter, params object[] components)
    {
        string text = "";
        for (int i = 0; i < components.Length; i++)
        {
            text += components[i].ToString();
            if (i < components.Length - 1)
            {
                text += delimiter;
            }
        }
        return text;
    }

    public static bool checkIP(string ip)
    {
        int num = ip.IndexOf('.');
        if (num == -1)
        {
            return false;
        }
        int num2 = ip.IndexOf('.', num + 1);
        if (num2 == -1)
        {
            return false;
        }
        int num3 = ip.IndexOf('.', num2 + 1);
        if (num3 == -1)
        {
            return false;
        }
        if (ip.IndexOf('.', num3 + 1) != -1)
        {
            return false;
        }
        return true;
    }

    public static bool TryGetUInt32FromIP(string ip, out uint value)
    {
        value = 0u;
        if (string.IsNullOrWhiteSpace(ip))
        {
            return false;
        }
        string[] array = ip.Split('.');
        if (array.Length != 4)
        {
            return false;
        }
        if (uint.TryParse(array[0], out var result))
        {
            value |= (result & 0xFF) << 24;
            if (uint.TryParse(array[1], out result))
            {
                value |= (result & 0xFF) << 16;
                if (uint.TryParse(array[2], out result))
                {
                    value |= (result & 0xFF) << 8;
                    if (uint.TryParse(array[3], out result))
                    {
                        value |= result & 0xFF;
                        return true;
                    }
                    return false;
                }
                return false;
            }
            return false;
        }
        return false;
    }

    public static uint getUInt32FromIP(string ip)
    {
        TryGetUInt32FromIP(ip, out var value);
        return value;
    }

    public static string getIPFromUInt32(uint ip)
    {
        return ((ip >> 24) & 0xFFu) + "." + ((ip >> 16) & 0xFFu) + "." + ((ip >> 8) & 0xFFu) + "." + (ip & 0xFFu);
    }
}
