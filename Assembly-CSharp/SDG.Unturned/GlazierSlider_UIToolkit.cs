using UnityEngine.UIElements;

namespace SDG.Unturned;

internal class GlazierSlider_UIToolkit : GlazierElementBase_UIToolkit, ISleekSlider, ISleekElement
{
    private ESleekOrientation _orientation = ESleekOrientation.VERTICAL;

    private SleekColor _backgroundColor = GlazierConst.DefaultSliderBackgroundColor;

    private SleekColor _foregroundColor = GlazierConst.DefaultSliderForegroundColor;

    private Slider control;

    private VisualElement trackerElement;

    private VisualElement draggerElement;

    public ESleekOrientation Orientation
    {
        get
        {
            return _orientation;
        }
        set
        {
            if (_orientation != value)
            {
                _orientation = value;
                UpdateOrientation();
            }
        }
    }

    public float Value
    {
        get
        {
            return control.value;
        }
        set
        {
            control.SetValueWithoutNotify(value);
        }
    }

    public SleekColor BackgroundColor
    {
        get
        {
            return _backgroundColor;
        }
        set
        {
            _backgroundColor = value;
            trackerElement.style.unityBackgroundImageTintColor = _backgroundColor;
        }
    }

    public SleekColor ForegroundColor
    {
        get
        {
            return _foregroundColor;
        }
        set
        {
            _foregroundColor = value;
            draggerElement.style.unityBackgroundImageTintColor = _foregroundColor;
        }
    }

    public bool IsInteractable
    {
        get
        {
            return control.enabledSelf;
        }
        set
        {
            control.SetEnabled(value);
        }
    }

    public event Dragged OnValueChanged;

    public GlazierSlider_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
        control = new Slider();
        control.userData = this;
        control.AddToClassList("unturned-slider");
        control.lowValue = 0f;
        control.highValue = 1f;
        control.RegisterValueChangedCallback(OnControlValueChanged);
        UpdateOrientation();
        VisualElement e = control.Q(null, "unity-base-slider__input").Q(null, "unity-base-slider__drag-container");
        trackerElement = e.Q(null, "unity-base-slider__tracker");
        draggerElement = e.Q(null, "unity-base-slider__dragger");
        visualElement = control;
    }

    internal override void SynchronizeColors()
    {
        trackerElement.style.unityBackgroundImageTintColor = _backgroundColor;
        draggerElement.style.unityBackgroundImageTintColor = _foregroundColor;
    }

    private void UpdateOrientation()
    {
        switch (_orientation)
        {
        case ESleekOrientation.HORIZONTAL:
            control.direction = SliderDirection.Horizontal;
            control.inverted = false;
            break;
        case ESleekOrientation.VERTICAL:
            control.direction = SliderDirection.Vertical;
            control.inverted = true;
            break;
        }
    }

    private void OnControlValueChanged(ChangeEvent<float> changeEvent)
    {
        this.OnValueChanged?.Invoke(this, changeEvent.newValue);
    }
}
