namespace SDG.Unturned;

internal class GlazierUInt16Field_IMGUI : GlazierNumericField_IMGUI, ISleekUInt16Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    private ushort _state;

    public ushort state
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

    public ushort minValue { get; set; }

    public ushort maxValue { get; set; } = ushort.MaxValue;


    public event TypedUInt16 onTypedUInt16;

    public GlazierUInt16Field_IMGUI()
    {
        state = 0;
    }

    protected override bool ParseNumericInput(string input)
    {
        if (ushort.TryParse(input, out var result))
        {
            result = MathfEx.Clamp(result, minValue, maxValue);
            if (_state != result)
            {
                _state = result;
                this.onTypedUInt16?.Invoke(this, _state);
            }
            return true;
        }
        return false;
    }
}
