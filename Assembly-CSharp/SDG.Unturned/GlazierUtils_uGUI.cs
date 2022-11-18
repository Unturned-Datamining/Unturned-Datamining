using TMPro;
using UnityEngine;

namespace SDG.Unturned;

internal static class GlazierUtils_uGUI
{
    public static TextAlignmentOptions TextAnchorToTMP(TextAnchor textAnchor)
    {
        return textAnchor switch
        {
            TextAnchor.LowerCenter => TextAlignmentOptions.Bottom, 
            TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft, 
            TextAnchor.LowerRight => TextAlignmentOptions.BottomRight, 
            TextAnchor.MiddleLeft => TextAlignmentOptions.Left, 
            TextAnchor.MiddleRight => TextAlignmentOptions.Right, 
            TextAnchor.UpperCenter => TextAlignmentOptions.Top, 
            TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft, 
            TextAnchor.UpperRight => TextAlignmentOptions.TopRight, 
            _ => TextAlignmentOptions.Center, 
        };
    }

    public static float GetFontSize(ESleekFontSize fontSize)
    {
        return fontSize switch
        {
            ESleekFontSize.Tiny => 8f, 
            ESleekFontSize.Small => 10f, 
            ESleekFontSize.Medium => 14f, 
            ESleekFontSize.Large => 20f, 
            ESleekFontSize.Title => 50f, 
            _ => 12f, 
        };
    }

    public static FontStyles GetFontStyleFlags(FontStyle fontStyle)
    {
        return fontStyle switch
        {
            FontStyle.Bold => FontStyles.Bold, 
            FontStyle.BoldAndItalic => FontStyles.Bold | FontStyles.Italic, 
            FontStyle.Italic => FontStyles.Italic, 
            _ => FontStyles.Normal, 
        };
    }

    public static float GetCharacterSpacing(ETextContrastStyle shadowStyle)
    {
        return shadowStyle switch
        {
            ETextContrastStyle.Outline => 10f, 
            ETextContrastStyle.Tooltip => 15f, 
            _ => 0f, 
        };
    }
}
