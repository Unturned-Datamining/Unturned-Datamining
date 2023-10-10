namespace SDG.Unturned;

internal class GlazierFloat32Field_IMGUI : GlazierNumericField_IMGUI, ISleekFloat32Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    private float _state;

    public float Value
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

    public event TypedSingle OnValueSubmitted;

    public event TypedSingle OnValueChanged;

    public GlazierFloat32Field_IMGUI()
    {
        Value = 0f;
    }

    protected override bool ParseNumericInput(string input)
    {
        if (input.Length > 0 && !char.IsDigit(input, input.Length - 1))
        {
            input += "0";
        }
        if (float.TryParse(input, out var result))
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

    protected override void OnReturnPressed()
    {
        this.OnValueSubmitted?.Invoke(this, _state);
    }
}
