namespace SDG.Unturned;

internal class GlazierUInt32Field_UIToolkit : GlazierNumericField_UIToolkit, ISleekUInt32Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    private uint _state;

    public uint Value
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

    public event TypedUInt32 OnValueChanged;

    public GlazierUInt32Field_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
    }

    protected override bool ParseNumericInput(string input)
    {
        bool flag;
        if (string.IsNullOrEmpty(input))
        {
            _state = 0u;
            flag = true;
        }
        else
        {
            flag = uint.TryParse(input, out _state);
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
        return Value.ToString();
    }
}
