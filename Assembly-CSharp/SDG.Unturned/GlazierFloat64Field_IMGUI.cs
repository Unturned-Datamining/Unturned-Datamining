namespace SDG.Unturned;

internal class GlazierFloat64Field_IMGUI : GlazierNumericField_IMGUI, ISleekFloat64Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    private double _state;

    public double Value
    {
        get
        {
            return _state;
        }
        set
        {
            _state = value;
            text = Value.ToString("F3");
        }
    }

    public event TypedDouble OnValueChanged;

    public GlazierFloat64Field_IMGUI()
    {
        Value = 0.0;
    }

    protected override bool ParseNumericInput(string input)
    {
        if (input.Length > 0 && !char.IsDigit(input, input.Length - 1))
        {
            input += "0";
        }
        if (double.TryParse(input, out var result))
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
