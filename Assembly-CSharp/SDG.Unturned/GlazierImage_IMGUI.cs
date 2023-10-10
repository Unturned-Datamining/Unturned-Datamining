using System;
using UnityEngine;

namespace SDG.Unturned;

internal class GlazierImage_IMGUI : GlazierElementBase_IMGUI, ISleekImage, ISleekElement
{
    public Texture Texture { get; set; }

    public float RotationAngle { get; set; }

    public bool CanRotate { get; set; }

    public bool ShouldDestroyTexture { get; set; }

    public SleekColor TintColor { get; set; } = ESleekTint.NONE;


    public event System.Action OnClicked;

    public event System.Action OnRightClicked;

    public void UpdateTexture(Texture2D newTexture)
    {
        Texture = newTexture;
    }

    public void SetTextureAndShouldDestroy(Texture2D texture, bool shouldDestroyTexture)
    {
        if (Texture != null && ShouldDestroyTexture)
        {
            UnityEngine.Object.Destroy(Texture);
        }
        Texture = texture;
        ShouldDestroyTexture = shouldDestroyTexture;
    }

    public override void InternalDestroy()
    {
        if (ShouldDestroyTexture && Texture != null)
        {
            UnityEngine.Object.DestroyImmediate(Texture);
            Texture = null;
        }
        base.InternalDestroy();
    }

    public override void OnGUI()
    {
        if (CanRotate)
        {
            GlazierUtils_IMGUI.drawAngledImageTexture(drawRect, Texture, RotationAngle, TintColor);
        }
        else
        {
            GlazierUtils_IMGUI.drawImageTexture(drawRect, Texture, TintColor);
        }
        ChildrenOnGUI();
        if (this.OnClicked == null && this.OnRightClicked == null)
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
                this.OnClicked?.Invoke();
            }
            else if (Event.current.button == 1)
            {
                this.OnRightClicked?.Invoke();
            }
        }
    }

    public GlazierImage_IMGUI(Texture texture)
    {
        Texture = texture;
    }
}
