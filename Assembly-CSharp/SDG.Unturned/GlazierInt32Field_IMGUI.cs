namespace SDG.Unturned;

internal class GlazierInt32Field_IMGUI : GlazierNumericField_IMGUI, ISleekInt32Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    private int _state;

    public int state
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

    public event TypedInt32 onTypedInt;

    public GlazierInt32Field_IMGUI()
    {
        state = 0;
    }

    protected override bool ParseNumericInput(string input)
    {
        if (int.TryParse(input, out var result))
        {
            if (_state != result)
            {
                _state = result;
                this.onTypedInt?.Invoke(this, _state);
            }
            return true;
        }
        return false;
    }
}
