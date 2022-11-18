namespace SDG.Unturned;

internal class GlazierFloat32Field_IMGUI : GlazierNumericField_IMGUI, ISleekFloat32Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    private float _state;

    public float state
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

    public event TypedSingle onEnteredSingle;

    public event TypedSingle onTypedSingle;

    public GlazierFloat32Field_IMGUI()
    {
        state = 0f;
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
                this.onTypedSingle?.Invoke(this, _state);
            }
            return true;
        }
        return false;
    }

    protected override void OnReturnPressed()
    {
        this.onEnteredSingle?.Invoke(this, _state);
    }
}
