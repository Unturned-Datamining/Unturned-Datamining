namespace SDG.Unturned;

internal class GlazierUInt8Field_IMGUI : GlazierNumericField_IMGUI, ISleekUInt8Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
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
            text = Value.ToString();
        }
    }

    public event TypedByte OnValueChanged;

    public GlazierUInt8Field_IMGUI()
    {
        Value = 0;
    }

    protected override bool ParseNumericInput(string input)
    {
        if (byte.TryParse(input, out var result))
        {
            if (_state != result)
            {
                _state = result;
                this.OnValueChanged?.Invoke(this, _state);
            }
            return true;
        }
        return false;
    }
}
