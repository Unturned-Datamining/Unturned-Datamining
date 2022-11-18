namespace SDG.Unturned;

internal class GlazierFloat64Field_IMGUI : GlazierNumericField_IMGUI, ISleekFloat64Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    private double _state;

    public double state
    {
        get
        {
            return _state;
        }
        set
        {
            _state = value;
            text = state.ToString("F3");
        }
    }

    public event TypedDouble onTypedDouble;

    public GlazierFloat64Field_IMGUI()
    {
        state = 0.0;
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
                this.onTypedDouble?.Invoke(this, _state);
            }
            return true;
        }
        return false;
    }
}
