namespace SDG.Unturned;

internal class GlazierUInt32Field_IMGUI : GlazierNumericField_IMGUI, ISleekUInt32Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
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
            text = Value.ToString();
        }
    }

    public event TypedUInt32 OnValueChanged;

    public GlazierUInt32Field_IMGUI()
    {
        Value = 0u;
    }

    protected override bool ParseNumericInput(string input)
    {
        if (uint.TryParse(input, out var result))
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
