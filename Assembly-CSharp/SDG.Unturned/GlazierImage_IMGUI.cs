using System;
using UnityEngine;

namespace SDG.Unturned;

internal class GlazierImage_IMGUI : GlazierElementBase_IMGUI, ISleekImage, ISleekElement
{
    public Texture texture { get; set; }

    public float angle { get; set; }

    public bool isAngled { get; set; }

    public bool shouldDestroyTexture { get; set; }

    public SleekColor color { get; set; } = ESleekTint.NONE;


    public event System.Action onImageClicked;

    public event System.Action onImageRightClicked;

    public void updateTexture(Texture2D newTexture)
    {
        texture = newTexture;
    }

    public void setTextureAndShouldDestroy(Texture2D texture, bool shouldDestroyTexture)
    {
        if (this.texture != null && this.shouldDestroyTexture)
        {
            UnityEngine.Object.Destroy(this.texture);
        }
        this.texture = texture;
        this.shouldDestroyTexture = shouldDestroyTexture;
    }

    public override void destroy()
    {
        if (shouldDestroyTexture && texture != null)
        {
            UnityEngine.Object.DestroyImmediate(texture);
            texture = null;
        }
        base.destroy();
    }

    public override void OnGUI()
    {
        if (isAngled)
        {
            GlazierUtils_IMGUI.drawAngledImageTexture(drawRect, texture, angle, color);
        }
        else
        {
            GlazierUtils_IMGUI.drawImageTexture(drawRect, texture, color);
        }
        ChildrenOnGUI();
        if (this.onImageClicked == null && this.onImageRightClicked == null)
        {
            return;
        }
        GUI.enabled = Event.current.type != EventType.Repaint && Event.current.type != EventType.Used;
        Color backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = ColorEx.BlackZeroAlpha;
        bool num = GUI.Button(drawRect, string.Empty);
        GUI.enabled = true;
        GUI.backgroundColor = backgroundColor;
        if (num)
        {
            if (Event.current.button == 0)
            {
                this.onImageClicked?.Invoke();
            }
            else if (Event.current.button == 1)
            {
                this.onImageRightClicked?.Invoke();
            }
        }
    }

    public GlazierImage_IMGUI(Texture texture)
    {
        this.texture = texture;
    }
}
