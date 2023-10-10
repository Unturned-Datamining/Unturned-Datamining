using TMPro;

namespace SDG.Unturned;

internal class GlazierFloat64Field_uGUI : GlazierNumericField_uGUI, ISleekFloat64Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
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

    public GlazierFloat64Field_uGUI(Glazier_uGUI glazier)
        : base(glazier)
    {
    }

    public override void ConstructNew()
    {
        base.ConstructNew();
        fieldComponent.contentType = TMP_InputField.ContentType.DecimalNumber;
        SynchronizeText();
    }

    protected override bool ParseNumericInput(string input)
    {
        if (input.Length > 0 && !char.IsDigit(input, input.Length - 1))
        {
            input += "0";
        }
        if (double.TryParse(input, out _state))
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
