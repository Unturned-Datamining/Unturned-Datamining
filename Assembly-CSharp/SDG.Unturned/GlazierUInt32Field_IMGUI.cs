namespace SDG.Unturned;

internal class GlazierUInt32Field_IMGUI : GlazierNumericField_IMGUI, ISleekUInt32Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    private uint _state;

    public uint state
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

    public event TypedUInt32 onTypedUInt32;

    public GlazierUInt32Field_IMGUI()
    {
        state = 0u;
    }

    protected override bool ParseNumericInput(string input)
    {
        if (uint.TryParse(input, out var result))
        {
            if (_state != result)
            {
                _state = result;
                this.onTypedUInt32?.Invoke(this, _state);
            }
            return true;
        }
        return false;
    }
}
