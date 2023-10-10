namespace SDG.Unturned;

internal class GlazierUInt16Field_UIToolkit : GlazierNumericField_UIToolkit, ISleekUInt16Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    private ushort _state;

    public ushort Value
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

    public ushort MinValue { get; set; }

    public ushort MaxValue { get; set; } = ushort.MaxValue;


    public event TypedUInt16 OnValueChanged;

    public GlazierUInt16Field_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
    }

    protected override bool ParseNumericInput(string input)
    {
        bool flag;
        if (string.IsNullOrEmpty(input))
        {
            _state = 0;
            flag = true;
        }
        else
        {
            flag = ushort.TryParse(input, out _state);
        }
        if (flag)
        {
            _state = MathfEx.Clamp(_state, MinValue, MaxValue);
            this.OnValueChanged?.Invoke(this, _state);
            return true;
        }
        return false;
    }

    protected override string NumberToString()
    {
        return Value.ToString();
    }
}
