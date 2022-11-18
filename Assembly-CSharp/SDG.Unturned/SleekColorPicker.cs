using UnityEngine;

namespace SDG.Unturned;

public class SleekColorPicker : SleekWrapper
{
    public ColorPicked onColorPicked;

    private ISleekImage colorImage;

    private ISleekUInt8Field rField;

    private ISleekUInt8Field gField;

    private ISleekUInt8Field bField;

    private ISleekSlider rSlider;

    private ISleekSlider gSlider;

    private ISleekSlider bSlider;

    private Color color;

    public Color state
    {
        get
        {
            return color;
        }
        set
        {
            color = value;
            updateColor();
            updateColorText();
            updateColorSlider();
        }
    }

    private void updateColor()
    {
        colorImage.color = color;
    }

    private void updateColorText()
    {
        rField.state = (byte)(color.r * 255f);
        gField.state = (byte)(color.g * 255f);
        bField.state = (byte)(color.b * 255f);
    }

    private void updateColorSlider()
    {
        rSlider.state = color.r;
        gSlider.state = color.g;
        bSlider.state = color.b;
    }

    private void onTypedRField(ISleekUInt8Field field, byte value)
    {
        color.r = (float)(int)value / 255f;
        updateColor();
        updateColorSlider();
        if (onColorPicked != null)
        {
            onColorPicked(this, color);
        }
    }

    private void onTypedGField(ISleekUInt8Field field, byte value)
    {
        color.g = (float)(int)value / 255f;
        updateColor();
        updateColorSlider();
        if (onColorPicked != null)
        {
            onColorPicked(this, color);
        }
    }

    private void onTypedBField(ISleekUInt8Field field, byte value)
    {
        color.b = (float)(int)value / 255f;
        updateColor();
        updateColorSlider();
        if (onColorPicked != null)
        {
            onColorPicked(this, color);
        }
    }

    private void onDraggedRSlider(ISleekSlider slider, float state)
    {
        color.r = state;
        updateColor();
        updateColorText();
        if (onColorPicked != null)
        {
            onColorPicked(this, color);
        }
    }

    private void onDraggedGSlider(ISleekSlider slider, float state)
    {
        color.g = state;
        updateColor();
        updateColorText();
        if (onColorPicked != null)
        {
            onColorPicked(this, color);
        }
    }

    private void onDraggedBSlider(ISleekSlider slider, float state)
    {
        color.b = state;
        updateColor();
        updateColorText();
        if (onColorPicked != null)
        {
            onColorPicked(this, color);
        }
    }

    public SleekColorPicker()
    {
        color = Color.black;
        base.sizeOffset_X = 240;
        base.sizeOffset_Y = 120;
        colorImage = Glazier.Get().CreateImage();
        colorImage.sizeOffset_X = 30;
        colorImage.sizeOffset_Y = 30;
        colorImage.texture = (Texture2D)GlazierResources.PixelTexture;
        AddChild(colorImage);
        rField = Glazier.Get().CreateUInt8Field();
        rField.positionOffset_X = 40;
        rField.sizeOffset_X = 60;
        rField.sizeOffset_Y = 30;
        rField.textColor = Palette.COLOR_R;
        rField.onTypedByte += onTypedRField;
        AddChild(rField);
        gField = Glazier.Get().CreateUInt8Field();
        gField.positionOffset_X = 110;
        gField.sizeOffset_X = 60;
        gField.sizeOffset_Y = 30;
        gField.textColor = Palette.COLOR_G;
        gField.onTypedByte += onTypedGField;
        AddChild(gField);
        bField = Glazier.Get().CreateUInt8Field();
        bField.positionOffset_X = 180;
        bField.sizeOffset_X = 60;
        bField.sizeOffset_Y = 30;
        bField.textColor = Palette.COLOR_B;
        bField.onTypedByte += onTypedBField;
        AddChild(bField);
        rSlider = Glazier.Get().CreateSlider();
        rSlider.positionOffset_X = 40;
        rSlider.positionOffset_Y = 40;
        rSlider.sizeOffset_X = 200;
        rSlider.sizeOffset_Y = 20;
        rSlider.orientation = ESleekOrientation.HORIZONTAL;
        rSlider.addLabel("R", Palette.COLOR_R, ESleekSide.LEFT);
        rSlider.onDragged += onDraggedRSlider;
        AddChild(rSlider);
        gSlider = Glazier.Get().CreateSlider();
        gSlider.positionOffset_X = 40;
        gSlider.positionOffset_Y = 70;
        gSlider.sizeOffset_X = 200;
        gSlider.sizeOffset_Y = 20;
        gSlider.orientation = ESleekOrientation.HORIZONTAL;
        gSlider.addLabel("G", Palette.COLOR_G, ESleekSide.LEFT);
        gSlider.onDragged += onDraggedGSlider;
        AddChild(gSlider);
        bSlider = Glazier.Get().CreateSlider();
        bSlider.positionOffset_X = 40;
        bSlider.positionOffset_Y = 100;
        bSlider.sizeOffset_X = 200;
        bSlider.sizeOffset_Y = 20;
        bSlider.orientation = ESleekOrientation.HORIZONTAL;
        bSlider.addLabel("B", Palette.COLOR_B, ESleekSide.LEFT);
        bSlider.onDragged += onDraggedBSlider;
        AddChild(bSlider);
    }
}
