namespace SDG.Unturned;

internal class GlazierUInt8Field_UIToolkit : GlazierNumericField_UIToolkit, ISleekUInt8Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    private byte _state;

    public byte Value
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

    public event TypedByte OnValueChanged;

    public GlazierUInt8Field_UIToolkit(Glazier_UIToolkit glazier)
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
            flag = byte.TryParse(input, out _state);
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
