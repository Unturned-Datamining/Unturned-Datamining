using System.Globalization;
using UnityEngine;

namespace SDG.Framework.Utilities;

public static class PaletteUtility
{
    public static string toRGB(Color color)
    {
        Color32 color2 = color;
        return "#" + color2.r.ToString("X2") + color2.g.ToString("X2") + color2.b.ToString("X2");
    }

    public static string toRGBA(Color color)
    {
        Color32 color2 = color;
        return "#" + color2.r.ToString("X2") + color2.g.ToString("X2") + color2.b.ToString("X2") + color2.a.ToString("X2");
    }

    public static bool tryParse(string value, out Color color)
    {
        color = Color.white;
        if (!string.IsNullOrEmpty(value))
        {
            uint result;
            switch (value.Length)
            {
            case 6:
                if (uint.TryParse(value, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out result))
                {
                    color.r = (int)(byte)((result >> 16) & 0xFF);
                    color.g = (int)(byte)((result >> 8) & 0xFF);
                    color.b = (int)(byte)(result & 0xFF);
                    color.a = 255f;
                    return true;
                }
                break;
            case 7:
                if (uint.TryParse(value.Substring(1, value.Length - 1), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out result))
                {
                    color.r = (int)(byte)((result >> 16) & 0xFF);
                    color.g = (int)(byte)((result >> 8) & 0xFF);
                    color.b = (int)(byte)(result & 0xFF);
                    color.a = 255f;
                    return true;
                }
                break;
            case 8:
                if (uint.TryParse(value, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out result))
                {
                    color.r = (int)(byte)((result >> 24) & 0xFF);
                    color.g = (int)(byte)((result >> 16) & 0xFF);
                    color.b = (int)(byte)((result >> 8) & 0xFF);
                    color.a = (int)(byte)(result & 0xFF);
                    return true;
                }
                break;
            case 9:
                if (uint.TryParse(value.Substring(1, value.Length - 1), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out result))
                {
                    color.r = (int)(byte)((result >> 24) & 0xFF);
                    color.g = (int)(byte)((result >> 16) & 0xFF);
                    color.b = (int)(byte)((result >> 8) & 0xFF);
                    color.a = (int)(byte)(result & 0xFF);
                    return true;
                }
                break;
            }
        }
        return false;
    }
}
