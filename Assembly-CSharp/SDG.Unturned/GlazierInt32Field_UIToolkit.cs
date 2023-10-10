namespace SDG.Unturned;

internal class GlazierInt32Field_UIToolkit : GlazierNumericField_UIToolkit, ISleekInt32Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
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
            SynchronizeText();
        }
    }

    public event TypedInt32 OnValueChanged;

    public GlazierInt32Field_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
    }

    protected override bool ParseNumericInput(string input)
    {
        bool flag;
        if (string.IsNullOrEmpty(input) || string.Equals(input, "-"))
        {
            _state = 0;
            flag = true;
        }
        else
        {
            flag = int.TryParse(input, out _state);
        }
        if (flag)
        {
            this.OnValueChanged?.Invoke(this, _state);
            return true;
        }
        return false;
    }

    protected override string NumberToString()
    {
        return Value.ToString();
    }
}
