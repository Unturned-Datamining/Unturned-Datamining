namespace SDG.Unturned;

internal class GlazierInt32Field_IMGUI : GlazierNumericField_IMGUI, ISleekInt32Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    private int _state;

    public int Value
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

    public event TypedInt32 OnValueChanged;

    public GlazierInt32Field_IMGUI()
    {
        Value = 0;
    }

    protected override bool ParseNumericInput(string input)
    {
        if (int.TryParse(input, out var result))
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
