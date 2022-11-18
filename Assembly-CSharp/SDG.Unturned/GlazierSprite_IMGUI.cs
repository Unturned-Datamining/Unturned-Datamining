using System;
using UnityEngine;

namespace SDG.Unturned;

internal class GlazierSprite_IMGUI : GlazierElementBase_IMGUI, ISleekSprite, ISleekElement
{
    private GUIStyle style;

    public Sprite sprite { get; set; }

    public SleekColor color { get; set; } = ESleekTint.NONE;


    public ESleekSpriteType drawMethod { get; set; }

    public bool isRaycastTarget { get; set; } = true;


    public event System.Action onImageClicked;

    public override void OnGUI()
    {
        if (sprite != null)
        {
            switch (drawMethod)
            {
            case ESleekSpriteType.Tiled:
                GlazierUtils_IMGUI.drawTile(drawRect, sprite.texture, color);
                break;
            case ESleekSpriteType.Sliced:
                if (style == null)
                {
                    style = new GUIStyle();
                    style.normal.background = sprite.texture;
                    style.border = new RectOffset(20, 20, 20, 20);
                }
                GlazierUtils_IMGUI.drawSliced(drawRect, sprite.texture, color, style);
                break;
            case ESleekSpriteType.Regular:
                GlazierUtils_IMGUI.drawImageTexture(drawRect, sprite.texture, color);
                break;
            }
        }
        ChildrenOnGUI();
        if (this.onImageClicked != null)
        {
            GUI.enabled = isRaycastTarget && Event.current.type != EventType.Repaint && Event.current.type != EventType.Used;
            Color backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = ColorEx.BlackZeroAlpha;
            bool num = GUI.Button(drawRect, string.Empty);
            GUI.enabled = true;
            GUI.backgroundColor = backgroundColor;
            if (num && Event.current.button == 0)
            {
                this.onImageClicked?.Invoke();
            }
        }
    }

    public GlazierSprite_IMGUI(Sprite sprite)
    {
        this.sprite = sprite;
    }
}
