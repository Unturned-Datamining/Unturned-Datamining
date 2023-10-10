namespace SDG.Unturned;

internal class GlazierUInt16Field_IMGUI : GlazierNumericField_IMGUI, ISleekUInt16Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
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
            text = Value.ToString();
        }
    }

    public ushort MinValue { get; set; }

    public ushort MaxValue { get; set; } = ushort.MaxValue;


    public event TypedUInt16 OnValueChanged;

    public GlazierUInt16Field_IMGUI()
    {
        Value = 0;
    }

    protected override bool ParseNumericInput(string input)
    {
        if (ushort.TryParse(input, out var result))
        {
            result = MathfEx.Clamp(result, MinValue, MaxValue);
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
