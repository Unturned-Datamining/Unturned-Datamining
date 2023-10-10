namespace SDG.Unturned;

internal class GlazierFloat64Field_UIToolkit : GlazierNumericField_UIToolkit, ISleekFloat64Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
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
            SynchronizeText();
        }
    }

    public event TypedDouble OnValueChanged;

    public GlazierFloat64Field_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
    }

    protected override bool ParseNumericInput(string input)
    {
        bool flag;
        if (string.IsNullOrEmpty(input) || string.Equals(input, "-"))
        {
            _state = 0.0;
            flag = true;
        }
        else
        {
            if (input.Length > 0 && !char.IsDigit(input, input.Length - 1))
            {
                input += "0";
            }
            flag = double.TryParse(input, out _state);
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
        return Value.ToString("F3");
    }
}
