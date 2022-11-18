using UnityEngine;

namespace SDG.Unturned;

public class Customization
{
    public static readonly byte FREE_CHARACTERS = 1;

    public static readonly byte PRO_CHARACTERS = 4;

    public static readonly byte FACES_FREE = 10;

    public static readonly byte HAIRS_FREE = 5;

    public static readonly byte BEARDS_FREE = 5;

    public static readonly byte FACES_PRO = 22;

    public static readonly byte HAIRS_PRO = 18;

    public static readonly byte BEARDS_PRO = 11;

    public static readonly Color[] SKINS = new Color[10]
    {
        new Color(0.95686275f, 46f / 51f, 0.8235294f),
        new Color(0.8509804f, 0.7921569f, 0.7058824f),
        new Color(38f / 51f, 0.64705884f, 26f / 51f),
        new Color(0.6156863f, 8f / 15f, 0.41960785f),
        new Color(0.5803922f, 0.4627451f, 0.29411766f),
        new Color(0.4392157f, 32f / 85f, 0.28627452f),
        new Color(0.3254902f, 0.2784314f, 18f / 85f),
        new Color(0.29411766f, 0.23921569f, 0.19215687f),
        new Color(0.2f, 0.17254902f, 0.14509805f),
        new Color(7f / 51f, 0.12156863f, 0.10980392f)
    };

    public static readonly Color[] COLORS = new Color[10]
    {
        new Color(43f / 51f, 43f / 51f, 43f / 51f),
        new Color(0.75686276f, 0.75686276f, 0.75686276f),
        new Color(41f / 51f, 64f / 85f, 28f / 51f),
        new Color(0.6745098f, 0.41568628f, 19f / 85f),
        new Color(0.4f, 16f / 51f, 11f / 51f),
        new Color(29f / 85f, 23f / 85f, 0.18431373f),
        new Color(0.2784314f, 19f / 85f, 8f / 51f),
        new Color(0.20784314f, 0.17254902f, 2f / 15f),
        new Color(11f / 51f, 11f / 51f, 11f / 51f),
        new Color(5f / 51f, 5f / 51f, 5f / 51f)
    };

    public static readonly Color[] MARKER_COLORS = new Color[6]
    {
        Palette.COLOR_B,
        Palette.COLOR_G,
        Palette.COLOR_O,
        Palette.COLOR_P,
        Palette.COLOR_R,
        Palette.COLOR_Y
    };

    public static readonly byte SKILLSETS = 11;

    public static bool checkSkin(Color color)
    {
        for (int i = 0; i < SKINS.Length; i++)
        {
            if (Mathf.Abs(color.r - SKINS[i].r) < 0.01f && Mathf.Abs(color.g - SKINS[i].g) < 0.01f && Mathf.Abs(color.b - SKINS[i].b) < 0.01f)
            {
                return true;
            }
        }
        return false;
    }

    public static bool checkColor(Color color)
    {
        for (int i = 0; i < COLORS.Length; i++)
        {
            if (Mathf.Abs(color.r - COLORS[i].r) < 0.01f && Mathf.Abs(color.g - COLORS[i].g) < 0.01f && Mathf.Abs(color.b - COLORS[i].b) < 0.01f)
            {
                return true;
            }
        }
        return false;
    }
}
