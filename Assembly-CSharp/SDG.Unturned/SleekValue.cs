namespace SDG.Unturned;

public class SleekValue : SleekWrapper
{
    public Valued onValued;

    private float _state;

    private ISleekFloat32Field field;

    private ISleekSlider slider;

    public float state
    {
        get
        {
            return _state;
        }
        set
        {
            _state = value;
            field.Value = state;
            slider.Value = state;
        }
    }

    private void onTypedSingleField(ISleekFloat32Field field, float state)
    {
        onValued?.Invoke(this, state);
        _state = state;
        slider.Value = state;
    }

    private void onDraggedSlider(ISleekSlider slider, float state)
    {
        onValued?.Invoke(this, state);
        _state = state;
        field.Value = state;
    }

    public SleekValue()
    {
        field = Glazier.Get().CreateFloat32Field();
        field.SizeOffset_X = -5f;
        field.SizeScale_X = 0.4f;
        field.SizeScale_Y = 1f;
        field.OnValueChanged += onTypedSingleField;
        AddChild(field);
        slider = Glazier.Get().CreateSlider();
        slider.PositionOffset_X = 5f;
        slider.PositionOffset_Y = -10f;
        slider.PositionScale_X = 0.4f;
        slider.PositionScale_Y = 0.5f;
        slider.SizeOffset_X = -5f;
        slider.SizeOffset_Y = 20f;
        slider.SizeScale_X = 0.6f;
        slider.Orientation = ESleekOrientation.HORIZONTAL;
        slider.OnValueChanged += onDraggedSlider;
        AddChild(slider);
    }
}
