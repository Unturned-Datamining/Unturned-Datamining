namespace SDG.Unturned;

internal class GlazierUInt8Field_IMGUI : GlazierNumericField_IMGUI, ISleekUInt8Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    private byte _state;

    public byte state
    {
        get
        {
            return _state;
        }
        set
        {
            _state = value;
            text = state.ToString();
        }
    }

    public event TypedByte onTypedByte;

    public GlazierUInt8Field_IMGUI()
    {
        state = 0;
    }

    protected override bool ParseNumericInput(string input)
    {
        if (byte.TryParse(input, out var result))
        {
            if (_state != result)
            {
                _state = result;
                this.onTypedByte?.Invoke(this, _state);
            }
            return true;
        }
        return false;
    }
}
