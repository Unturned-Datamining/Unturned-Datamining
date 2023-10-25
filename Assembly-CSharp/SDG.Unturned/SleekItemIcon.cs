using UnityEngine;

namespace SDG.Unturned;

public class SleekItemIcon : SleekWrapper
{
    private ISleekImage internalImage;

    public byte rot
    {
        set
        {
            internalImage.RotationAngle = value * 90;
        }
    }

    public bool isAngled
    {
        set
        {
            internalImage.CanRotate = value;
        }
    }

    public SleekColor color
    {
        get
        {
            return internalImage.TintColor;
        }
        set
        {
            internalImage.TintColor = value;
        }
    }

    /// <summary>
    /// Hide existing icon until refresh.
    /// Experimented with doing this for every refresh, but it looks bad in particular for hotbar.
    /// </summary>
    public void Clear()
    {
        internalImage.Texture = null;
    }

    public void Refresh(ushort id, byte quality, byte[] state)
    {
        ItemTool.getIcon(id, quality, state, OnIconReady);
    }

    public void Refresh(ushort id, byte quality, byte[] state, ItemAsset itemAsset)
    {
        ItemTool.getIcon(id, quality, state, itemAsset, OnIconReady);
    }

    public void Refresh(ushort id, byte quality, byte[] state, ItemAsset itemAsset, int widthOverride, int heightOverride)
    {
        ItemTool.getIcon(id, quality, state, itemAsset, widthOverride, heightOverride, OnIconReady);
    }

    public override void OnDestroy()
    {
        internalImage = null;
    }

    public SleekItemIcon()
    {
        internalImage = Glazier.Get().CreateImage();
        internalImage.SizeScale_X = 1f;
        internalImage.SizeScale_Y = 1f;
        AddChild(internalImage);
    }

    private void OnIconReady(Texture2D texture)
    {
        if (internalImage != null)
        {
            internalImage.Texture = texture;
        }
    }
}
