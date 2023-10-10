using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned;

internal class GlazierToggle_UIToolkit : GlazierElementBase_UIToolkit, ISleekToggle, ISleekElement, ISleekWithTooltip
{
    private SleekColor _backgroundColor = GlazierConst.DefaultToggleBackgroundColor;

    private SleekColor _foregroundColor = GlazierConst.DefaultToggleForegroundColor;

    private Toggle control;

    private VisualElement backgroundElement;

    private VisualElement checkmarkElement;

    public bool Value
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

    public string TooltipText { get; set; }

    public SleekColor BackgroundColor
    {
        get
        {
            return _backgroundColor;
        }
        set
        {
            _backgroundColor = value;
            backgroundElement.style.unityBackgroundImageTintColor = _backgroundColor;
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
            checkmarkElement.style.unityBackgroundImageTintColor = _foregroundColor;
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

    public event Toggled OnValueChanged;

    public GlazierToggle_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
        base.SizeOffset_X = 40f;
        base.SizeOffset_Y = 40f;
        control = new Toggle();
        control.userData = this;
        control.AddToClassList("unturned-toggle");
        control.RegisterValueChangedCallback(OnControlValueChanged);
        backgroundElement = control.Q(null, "unity-toggle__input");
        checkmarkElement = control.Q("unity-checkmark");
        visualElement = control;
    }

    internal override void SynchronizeColors()
    {
        backgroundElement.style.unityBackgroundImageTintColor = _backgroundColor;
        checkmarkElement.style.unityBackgroundImageTintColor = _foregroundColor;
    }

    internal override bool GetTooltipParameters(out string tooltipText, out Color tooltipColor)
    {
        tooltipText = TooltipText;
        tooltipColor = new SleekColor(ESleekTint.FONT);
        return true;
    }

    private void OnControlValueChanged(ChangeEvent<bool> changeEvent)
    {
        this.OnValueChanged?.Invoke(this, changeEvent.newValue);
    }
}
