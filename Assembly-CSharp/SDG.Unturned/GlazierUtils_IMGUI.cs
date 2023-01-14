using UnityEngine;

namespace SDG.Unturned;

internal static class GlazierUtils_IMGUI
{
    public static bool allowInput = true;

    private static Vector2[] outlineOffsets_4way = new Vector2[4]
    {
        new Vector2(0f, 1f),
        new Vector2(1f, 0f),
        new Vector2(0f, -1f),
        new Vector2(-1f, 0f)
    };

    private static Vector2[] outlineOffsets_8way = new Vector2[8]
    {
        new Vector2(0f, 1f),
        new Vector2(0.707f, 0.707f),
        new Vector2(1f, 0f),
        new Vector2(0.707f, -0.707f),
        new Vector2(0f, -1f),
        new Vector2(-0.707f, -0.707f),
        new Vector2(-1f, 0f),
        new Vector2(-0.707f, 0.707f)
    };

    private static int controlNameCounter = -1;

    public static int getScaledFontSize(int originalFontSize)
    {
        return Mathf.CeilToInt((float)originalFontSize * GraphicsSettings.userInterfaceScale);
    }

    public static void drawAngledImageTexture(Rect area, Texture texture, float angle, Color color)
    {
        if (texture != null)
        {
            if (!GUI.enabled)
            {
                color.a *= 0.5f;
            }
            GUI.color = color;
            Matrix4x4 matrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, area.center);
            GUI.DrawTexture(area, texture, ScaleMode.StretchToFill);
            GUI.matrix = matrix;
            GUI.color = Color.white;
        }
    }

    public static void drawImageTexture(Rect area, Texture texture, Color color)
    {
        if (texture != null)
        {
            if (!GUI.enabled)
            {
                color.a *= 0.5f;
            }
            GUI.color = color;
            GUI.DrawTexture(area, texture, ScaleMode.StretchToFill);
            GUI.color = Color.white;
        }
    }

    public static void drawTile(Rect area, Texture texture, Color color)
    {
        if (texture != null)
        {
            if (!GUI.enabled)
            {
                color.a *= 0.5f;
            }
            GUI.color = color;
            float userInterfaceScale = GraphicsSettings.userInterfaceScale;
            GUI.DrawTextureWithTexCoords(area, texture, new Rect(0f, 0f, area.width / (float)texture.width / userInterfaceScale, area.height / (float)texture.height / userInterfaceScale));
            GUI.color = Color.white;
        }
    }

    public static void drawSliced(Rect area, Texture texture, Color color, GUIStyle style)
    {
        if (texture != null)
        {
            if (!GUI.enabled)
            {
                color.a *= 0.5f;
            }
            GUI.backgroundColor = color;
            GUI.Box(area, string.Empty, style);
            GUI.color = Color.white;
        }
    }

    public static bool drawToggle(Rect area, Color color, bool state, GUIContent content)
    {
        GUI.backgroundColor = color;
        state = GUI.Toggle(area, state, content);
        return state;
    }

    public static bool drawButton(Rect area, Color color)
    {
        if (allowInput)
        {
            GUI.backgroundColor = color;
            return GUI.Button(area, "");
        }
        drawBox(area, color);
        return false;
    }

    public static void drawBox(Rect area, Color color)
    {
        GUI.backgroundColor = color;
        GUI.Box(area, "");
    }

    public static void drawLabel(Rect area, FontStyle fontStyle, TextAnchor fontAlignment, int fontSize, GUIContent shadowContent, Color color, GUIContent content, ETextContrastContext shadowStyle)
    {
        GUI.skin.label.fontStyle = fontStyle;
        GUI.skin.label.alignment = fontAlignment;
        GUI.skin.label.fontSize = getScaledFontSize(fontSize);
        bool richText = GUI.skin.label.richText;
        GUI.skin.label.richText = shadowContent != null;
        if (shadowContent == null)
        {
            drawLabelOutline(area, content, SleekShadowStyle.ContextToStyle(shadowStyle), color.a);
        }
        else
        {
            drawLabelOutline(area, shadowContent, SleekShadowStyle.ContextToStyle(shadowStyle), color.a);
        }
        GUI.contentColor = color;
        GUI.Label(area, content);
        GUI.skin.label.richText = richText;
    }

    public static void drawLabel(Rect area, FontStyle fontStyle, TextAnchor fontAlignment, int fontSize, bool isRich, Color color, string text, ETextContrastContext shadowStyle)
    {
        GUI.skin.label.fontStyle = fontStyle;
        GUI.skin.label.alignment = fontAlignment;
        GUI.skin.label.fontSize = getScaledFontSize(fontSize);
        if (isRich)
        {
            bool richText = GUI.skin.label.richText;
            GUI.skin.label.richText = isRich;
            GUI.Label(area, text);
            GUI.skin.label.richText = richText;
        }
        else
        {
            drawLabelOutline(area, text, SleekShadowStyle.ContextToStyle(shadowStyle), color.a);
            GUI.contentColor = color;
            GUI.Label(area, text);
        }
    }

    public static string drawField(Rect area, FontStyle fontStyle, TextAnchor fontAlignment, int fontSize, Color color_0, Color color_1, string text, int maxLength, bool multiline, ETextContrastContext shadowStyle)
    {
        return DrawTextInputField(area, fontStyle, fontAlignment, fontSize, color_0, color_1, text, maxLength, string.Empty, multiline, shadowStyle);
    }

    public static string DrawTextInputField(Rect area, FontStyle fontStyle, TextAnchor fontAlignment, int fontSize, Color color_0, Color color_1, string text, int maxLength, string hint, bool multiline, ETextContrastContext shadowStyle)
    {
        GUI.skin.textArea.fontStyle = fontStyle;
        GUI.skin.textArea.alignment = fontAlignment;
        GUI.skin.textArea.fontSize = getScaledFontSize(fontSize);
        GUI.skin.textField.fontStyle = fontStyle;
        GUI.skin.textField.alignment = fontAlignment;
        GUI.skin.textField.fontSize = getScaledFontSize(fontSize);
        GUI.backgroundColor = color_0;
        GUI.contentColor = color_1;
        if (allowInput)
        {
            if (text == null)
            {
                text = string.Empty;
            }
            text = ((maxLength > 0) ? ((!multiline) ? GUI.TextField(area, text, maxLength) : GUI.TextArea(area, text, maxLength)) : ((!multiline) ? GUI.TextField(area, text) : GUI.TextArea(area, text)));
            if (text.Length < 1)
            {
                drawLabel(area, fontStyle, fontAlignment, fontSize, isRich: false, color_1 * 0.5f, hint, shadowStyle);
            }
            return text;
        }
        drawBox(area, color_0);
        drawLabel(area, fontStyle, fontAlignment, fontSize, isRich: false, color_1, text, shadowStyle);
        return text;
    }

    public static string DrawPasswordField(Rect area, FontStyle fontStyle, TextAnchor fontAlignment, int fontSize, Color color_0, Color color_1, string text, int maxLength, string hint, char replace, ETextContrastContext shadowStyle)
    {
        GUI.skin.textField.fontStyle = fontStyle;
        GUI.skin.textField.alignment = fontAlignment;
        GUI.skin.textField.fontSize = getScaledFontSize(fontSize);
        GUI.backgroundColor = color_0;
        GUI.contentColor = color_1;
        if (allowInput)
        {
            if (text == null)
            {
                text = string.Empty;
            }
            text = ((maxLength <= 0) ? GUI.PasswordField(area, text, replace) : GUI.PasswordField(area, text, replace, maxLength));
            if (text.Length < 1)
            {
                drawLabel(area, fontStyle, fontAlignment, fontSize, isRich: false, color_1 * 0.5f, hint, shadowStyle);
            }
            return text;
        }
        drawBox(area, color_0);
        string text2 = string.Empty;
        if (text != null)
        {
            for (int i = 0; i < text.Length; i++)
            {
                text2 += replace;
            }
        }
        drawLabel(area, fontStyle, fontAlignment, fontSize, isRich: false, color_1, text2, shadowStyle);
        return text;
    }

    public static float drawSlider(Rect area, ESleekOrientation orientation, float state, float size, Color color)
    {
        GUI.backgroundColor = color;
        state = ((orientation != 0) ? GUI.VerticalScrollbar(area, state, size, 0f, 1f) : GUI.HorizontalScrollbar(area, state, size, 0f, 1f));
        return state;
    }

    private static void drawLabelOutline(Rect area, GUIContent content, Vector2[] offsets, float magnitude)
    {
        foreach (Vector2 vector in offsets)
        {
            GUI.Label(new Rect(area.position + vector * magnitude, area.size), content);
        }
    }

    private static void drawLabelOutline(Rect area, GUIContent content, ETextContrastStyle shadowStyle, float alpha)
    {
        Color shadowColor = OptionsSettings.shadowColor;
        switch (shadowStyle)
        {
        case ETextContrastStyle.None:
            break;
        case ETextContrastStyle.Shadow:
            shadowColor.a = 0.5f * alpha;
            GUI.contentColor = shadowColor;
            area.x++;
            area.y++;
            GUI.Label(area, content);
            break;
        case ETextContrastStyle.Outline:
            shadowColor.a = 0.5f * alpha;
            GUI.contentColor = shadowColor;
            drawLabelOutline(area, content, outlineOffsets_4way, 1f);
            break;
        case ETextContrastStyle.Tooltip:
            shadowColor.a = 0.5f * alpha;
            GUI.contentColor = shadowColor;
            drawLabelOutline(area, content, outlineOffsets_8way, 2f);
            shadowColor.a = 1f * alpha;
            GUI.contentColor = shadowColor;
            drawLabelOutline(area, content, outlineOffsets_8way, 1f);
            break;
        }
    }

    private static void drawLabelOutline(Rect area, string text, Vector2[] offsets, float magnitude)
    {
        foreach (Vector2 vector in offsets)
        {
            GUI.Label(new Rect(area.position + vector * magnitude, area.size), text);
        }
    }

    private static void drawLabelOutline(Rect area, string text, ETextContrastStyle shadowStyle, float alpha)
    {
        Color shadowColor = OptionsSettings.shadowColor;
        switch (shadowStyle)
        {
        case ETextContrastStyle.None:
            break;
        case ETextContrastStyle.Shadow:
            shadowColor.a = 0.5f * alpha;
            GUI.contentColor = shadowColor;
            area.x++;
            area.y++;
            GUI.Label(area, text);
            break;
        case ETextContrastStyle.Outline:
            shadowColor.a = 0.5f * alpha;
            GUI.contentColor = shadowColor;
            drawLabelOutline(area, text, outlineOffsets_4way, 1f);
            break;
        case ETextContrastStyle.Tooltip:
            shadowColor.a = 0.5f * alpha;
            GUI.contentColor = shadowColor;
            drawLabelOutline(area, text, outlineOffsets_8way, 2f);
            shadowColor.a = 1f * alpha;
            GUI.contentColor = shadowColor;
            drawLabelOutline(area, text, outlineOffsets_8way, 1f);
            break;
        }
    }

    public static int GetFontSize(ESleekFontSize fontSize)
    {
        return fontSize switch
        {
            ESleekFontSize.Tiny => 8, 
            ESleekFontSize.Small => 10, 
            ESleekFontSize.Medium => 14, 
            ESleekFontSize.Large => 20, 
            ESleekFontSize.Title => 50, 
            _ => 12, 
        };
    }

    public static string CreateUniqueControlName()
    {
        controlNameCounter++;
        return "Glazier" + controlNameCounter;
    }
}
