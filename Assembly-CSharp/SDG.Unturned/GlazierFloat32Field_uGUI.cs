using TMPro;

namespace SDG.Unturned;

internal class GlazierFloat32Field_uGUI : GlazierNumericField_uGUI, ISleekFloat32Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    private float _state;

    public float Value
    {
        get
        {
            return _state;
        }
        set
        {
            _state = value;
            SynchronizeText();
        }
    }

    public event TypedSingle OnValueSubmitted;

    public event TypedSingle OnValueChanged;

    public GlazierFloat32Field_uGUI(Glazier_uGUI glazier)
        : base(glazier)
    {
    }

    public override void ConstructNew()
    {
        base.ConstructNew();
        fieldComponent.contentType = TMP_InputField.ContentType.DecimalNumber;
        SynchronizeText();
    }

    protected override void OnUnitySubmit(string input)
    {
        this.OnValueSubmitted?.Invoke(this, _state);
    }

    protected override bool ParseNumericInput(string input)
    {
        if (input.Length > 0 && !char.IsDigit(input, input.Length - 1))
        {
            input += "0";
        }
        if (float.TryParse(input, out _state))
        {
            this.OnValueChanged?.Invoke(this, _state);
            return true;
        }
        return false;
    }

    protected override string NumberToString()
    {
        return Value.ToString("F3");
    }
}
