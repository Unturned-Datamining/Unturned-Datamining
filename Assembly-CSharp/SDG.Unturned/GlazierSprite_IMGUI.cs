using System;
using UnityEngine;

namespace SDG.Unturned;

internal class GlazierSprite_IMGUI : GlazierElementBase_IMGUI, ISleekSprite, ISleekElement
{
    private GUIStyle style;

    public Sprite Sprite { get; set; }

    public SleekColor TintColor { get; set; } = ESleekTint.NONE;


    public ESleekSpriteType DrawMethod { get; set; }

    public bool IsRaycastTarget { get; set; } = true;


    public Vector2Int TileRepeatHintForUITK { get; set; }

    public event System.Action OnClicked;

    public override void OnGUI()
    {
        if (Sprite != null)
        {
            switch (DrawMethod)
            {
            case ESleekSpriteType.Tiled:
                GlazierUtils_IMGUI.drawTile(drawRect, Sprite.texture, TintColor);
                break;
            case ESleekSpriteType.Sliced:
                if (style == null)
                {
                    style = new GUIStyle();
                    style.normal.background = Sprite.texture;
                    style.border = new RectOffset(20, 20, 20, 20);
                }
                GlazierUtils_IMGUI.drawSliced(drawRect, Sprite.texture, TintColor, style);
                break;
            case ESleekSpriteType.Regular:
                GlazierUtils_IMGUI.drawImageTexture(drawRect, Sprite.texture, TintColor);
                break;
            }
        }
        ChildrenOnGUI();
        if (this.OnClicked != null)
        {
            GUI.enabled = IsRaycastTarget && Event.current.type != EventType.Repaint && Event.current.type != EventType.Used;
            Color backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = ColorEx.BlackZeroAlpha;
            bool num = GUI.Button(drawRect, string.Empty);
            GUI.enabled = true;
            GUI.backgroundColor = backgroundColor;
            if (num && Event.current.button == 0)
            {
                this.OnClicked?.Invoke();
            }
        }
    }

    public GlazierSprite_IMGUI(Sprite sprite)
    {
        Sprite = sprite;
    }
}
