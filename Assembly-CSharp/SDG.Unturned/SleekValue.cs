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
            field.state = state;
            slider.state = state;
        }
    }

    private void onTypedSingleField(ISleekFloat32Field field, float state)
    {
        if (onValued != null)
        {
            onValued(this, state);
        }
        _state = state;
        slider.state = state;
    }

    private void onDraggedSlider(ISleekSlider slider, float state)
    {
        if (onValued != null)
        {
            onValued(this, state);
        }
        _state = state;
        field.state = state;
    }

    public SleekValue()
    {
        field = Glazier.Get().CreateFloat32Field();
        field.sizeOffset_X = -5;
        field.sizeScale_X = 0.4f;
        field.sizeScale_Y = 1f;
        field.onTypedSingle += onTypedSingleField;
        AddChild(field);
        slider = Glazier.Get().CreateSlider();
        slider.positionOffset_X = 5;
        slider.positionOffset_Y = -10;
        slider.positionScale_X = 0.4f;
        slider.positionScale_Y = 0.5f;
        slider.sizeOffset_X = -5;
        slider.sizeOffset_Y = 20;
        slider.sizeScale_X = 0.6f;
        slider.orientation = ESleekOrientation.HORIZONTAL;
        slider.onDragged += onDraggedSlider;
        AddChild(slider);
    }
}
