using System.Globalization;
using UnityEngine;

namespace SDG.Unturned;

public class Palette
{
    public static readonly Color SERVER = Color.green;

    public static readonly Color ADMIN = Color.cyan;

    public static readonly Color PRO = new Color(0.8235294f, 0.7490196f, 2f / 15f);

    public static readonly Color COLOR_W = new Color(0.7058824f, 0.7058824f, 0.7058824f);

    public static readonly Color COLOR_R = new Color(0.7490196f, 0.12156863f, 0.12156863f);

    public static readonly Color COLOR_G = new Color(0.12156863f, 0.5294118f, 0.12156863f);

    public static readonly Color COLOR_B = new Color(10f / 51f, 0.59607846f, 40f / 51f);

    public static readonly Color COLOR_O = new Color(57f / 85f, 0.5019608f, 5f / 51f);

    public static readonly Color COLOR_Y = new Color(44f / 51f, 0.7058824f, 0.07450981f);

    public static readonly Color COLOR_P = new Color(0.41568628f, 14f / 51f, 0.42745098f);

    public static readonly Color AMBIENT = new Color(0.7f, 0.7f, 0.7f);

    public static readonly Color MYTHICAL = new Color(50f / 51f, 10f / 51f, 5f / 51f);

    public static string hex(Color32 color)
    {
        return "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
    }

    public static Color hex(string color)
    {
        if (!string.IsNullOrEmpty(color) && color.Length == 7 && uint.TryParse(color.Substring(1, color.Length - 1), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var result))
        {
            uint num = (result >> 16) & 0xFF;
            uint num2 = (result >> 8) & 0xFFu;
            uint num3 = result & 0xFFu;
            return new Color32((byte)num, (byte)num2, (byte)num3, byte.MaxValue);
        }
        return Color.white;
    }
}
