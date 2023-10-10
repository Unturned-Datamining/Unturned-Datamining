namespace SDG.Unturned;

internal class GlazierFloat32Field_UIToolkit : GlazierNumericField_UIToolkit, ISleekFloat32Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
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

    public GlazierFloat32Field_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
    }

    protected override void OnSubmitted()
    {
        this.OnValueSubmitted?.Invoke(this, Value);
    }

    protected override bool ParseNumericInput(string input)
    {
        bool flag;
        if (string.IsNullOrEmpty(input) || string.Equals(input, "-"))
        {
            _state = 0f;
            flag = true;
        }
        else
        {
            if (input.Length > 0 && !char.IsDigit(input, input.Length - 1))
            {
                input += "0";
            }
            flag = float.TryParse(input, out _state);
        }
        if (flag)
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
