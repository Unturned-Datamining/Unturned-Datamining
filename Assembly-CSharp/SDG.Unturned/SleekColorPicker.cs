using UnityEngine;

namespace SDG.Unturned;

public class SleekColorPicker : SleekWrapper
{
    public ColorPicked onColorPicked;

    private ISleekImage colorImage;

    private ISleekUInt8Field rField;

    private ISleekUInt8Field gField;

    private ISleekUInt8Field bField;

    private ISleekUInt8Field aField;

    private ISleekSlider rSlider;

    private ISleekSlider gSlider;

    private ISleekSlider bSlider;

    private ISleekSlider aSlider;

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
        colorImage.TintColor = color;
    }

    private void updateColorText()
    {
        rField.Value = (byte)(color.r * 255f);
        gField.Value = (byte)(color.g * 255f);
        bField.Value = (byte)(color.b * 255f);
        aField.Value = (byte)(color.a * 255f);
    }

    private void updateColorSlider()
    {
        rSlider.Value = color.r;
        gSlider.Value = color.g;
        bSlider.Value = color.b;
        aSlider.Value = color.a;
    }

    private void onTypedRField(ISleekUInt8Field field, byte value)
    {
        color.r = (float)(int)value / 255f;
        updateColor();
        updateColorSlider();
        onColorPicked?.Invoke(this, color);
    }

    private void onTypedGField(ISleekUInt8Field field, byte value)
    {
        color.g = (float)(int)value / 255f;
        updateColor();
        updateColorSlider();
        onColorPicked?.Invoke(this, color);
    }

    private void onTypedBField(ISleekUInt8Field field, byte value)
    {
        color.b = (float)(int)value / 255f;
        updateColor();
        updateColorSlider();
        onColorPicked?.Invoke(this, color);
    }

    private void onTypedAField(ISleekUInt8Field field, byte value)
    {
        color.a = (float)(int)value / 255f;
        updateColor();
        updateColorSlider();
        onColorPicked?.Invoke(this, color);
    }

    private void onDraggedRSlider(ISleekSlider slider, float state)
    {
        color.r = state;
        updateColor();
        updateColorText();
        onColorPicked?.Invoke(this, color);
    }

    private void onDraggedGSlider(ISleekSlider slider, float state)
    {
        color.g = state;
        updateColor();
        updateColorText();
        onColorPicked?.Invoke(this, color);
    }

    private void onDraggedBSlider(ISleekSlider slider, float state)
    {
        color.b = state;
        updateColor();
        updateColorText();
        onColorPicked?.Invoke(this, color);
    }

    private void onDraggedASlider(ISleekSlider slider, float state)
    {
        color.a = state;
        updateColor();
        updateColorText();
        onColorPicked?.Invoke(this, color);
    }

    public void SetAllowAlpha(bool allowAlpha)
    {
        aField.IsVisible = allowAlpha;
        aSlider.IsVisible = allowAlpha;
        if (allowAlpha)
        {
            base.SizeOffset_Y = 150f;
            rField.SizeOffset_X = 50f;
            gField.PositionOffset_X = rField.PositionOffset_X + rField.SizeOffset_X;
            gField.SizeOffset_X = 50f;
            bField.PositionOffset_X = gField.PositionOffset_X + gField.SizeOffset_X;
            bField.SizeOffset_X = 50f;
            aField.PositionOffset_X = bField.PositionOffset_X + bField.SizeOffset_X;
        }
        else
        {
            base.SizeOffset_Y = 120f;
            rField.SizeOffset_X = 60f;
            gField.PositionOffset_X = rField.PositionOffset_X + rField.SizeOffset_X + 10f;
            gField.SizeOffset_X = 60f;
            bField.PositionOffset_X = gField.PositionOffset_X + gField.SizeOffset_X + 10f;
            bField.SizeOffset_X = 60f;
        }
    }

    public SleekColorPicker()
    {
        color = Color.black;
        base.SizeOffset_X = 240f;
        colorImage = Glazier.Get().CreateImage();
        colorImage.SizeOffset_X = 30f;
        colorImage.SizeOffset_Y = 30f;
        colorImage.Texture = (Texture2D)GlazierResources.PixelTexture;
        AddChild(colorImage);
        rField = Glazier.Get().CreateUInt8Field();
        rField.PositionOffset_X = 40f;
        rField.SizeOffset_Y = 30f;
        rField.TextColor = Palette.COLOR_R;
        rField.OnValueChanged += onTypedRField;
        AddChild(rField);
        gField = Glazier.Get().CreateUInt8Field();
        gField.SizeOffset_Y = 30f;
        gField.TextColor = Palette.COLOR_G;
        gField.OnValueChanged += onTypedGField;
        AddChild(gField);
        bField = Glazier.Get().CreateUInt8Field();
        bField.SizeOffset_Y = 30f;
        bField.TextColor = Palette.COLOR_B;
        bField.OnValueChanged += onTypedBField;
        AddChild(bField);
        aField = Glazier.Get().CreateUInt8Field();
        aField.SizeOffset_X = 50f;
        aField.SizeOffset_Y = 30f;
        aField.TextColor = Palette.COLOR_W;
        aField.OnValueChanged += onTypedAField;
        aField.IsVisible = false;
        AddChild(aField);
        rSlider = Glazier.Get().CreateSlider();
        rSlider.PositionOffset_X = 40f;
        rSlider.PositionOffset_Y = 40f;
        rSlider.SizeOffset_X = 200f;
        rSlider.SizeOffset_Y = 20f;
        rSlider.Orientation = ESleekOrientation.HORIZONTAL;
        rSlider.AddLabel("R", Palette.COLOR_R, ESleekSide.LEFT);
        rSlider.OnValueChanged += onDraggedRSlider;
        AddChild(rSlider);
        gSlider = Glazier.Get().CreateSlider();
        gSlider.PositionOffset_X = 40f;
        gSlider.PositionOffset_Y = 70f;
        gSlider.SizeOffset_X = 200f;
        gSlider.SizeOffset_Y = 20f;
        gSlider.Orientation = ESleekOrientation.HORIZONTAL;
        gSlider.AddLabel("G", Palette.COLOR_G, ESleekSide.LEFT);
        gSlider.OnValueChanged += onDraggedGSlider;
        AddChild(gSlider);
        bSlider = Glazier.Get().CreateSlider();
        bSlider.PositionOffset_X = 40f;
        bSlider.PositionOffset_Y = 100f;
        bSlider.SizeOffset_X = 200f;
        bSlider.SizeOffset_Y = 20f;
        bSlider.Orientation = ESleekOrientation.HORIZONTAL;
        bSlider.AddLabel("B", Palette.COLOR_B, ESleekSide.LEFT);
        bSlider.OnValueChanged += onDraggedBSlider;
        AddChild(bSlider);
        aSlider = Glazier.Get().CreateSlider();
        aSlider.PositionOffset_X = 40f;
        aSlider.PositionOffset_Y = 130f;
        aSlider.SizeOffset_X = 200f;
        aSlider.SizeOffset_Y = 20f;
        aSlider.Orientation = ESleekOrientation.HORIZONTAL;
        aSlider.AddLabel("A", Palette.COLOR_W, ESleekSide.LEFT);
        aSlider.OnValueChanged += onDraggedASlider;
        aSlider.IsVisible = false;
        AddChild(aSlider);
        SetAllowAlpha(allowAlpha: false);
    }
}
