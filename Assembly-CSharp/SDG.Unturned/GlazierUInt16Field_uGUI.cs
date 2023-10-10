using TMPro;

namespace SDG.Unturned;

internal class GlazierUInt16Field_uGUI : GlazierNumericField_uGUI, ISleekUInt16Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
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
            SynchronizeText();
        }
    }

    public ushort MinValue { get; set; }

    public ushort MaxValue { get; set; } = ushort.MaxValue;


    public event TypedUInt16 OnValueChanged;

    public GlazierUInt16Field_uGUI(Glazier_uGUI glazier)
        : base(glazier)
    {
    }

    public override void ConstructNew()
    {
        base.ConstructNew();
        fieldComponent.contentType = TMP_InputField.ContentType.IntegerNumber;
        SynchronizeText();
    }

    protected override bool ParseNumericInput(string input)
    {
        if (ushort.TryParse(input, out _state))
        {
            _state = MathfEx.Clamp(_state, MinValue, MaxValue);
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
