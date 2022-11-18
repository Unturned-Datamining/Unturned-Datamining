using TMPro;

namespace SDG.Unturned;

internal class GlazierUInt16Field_uGUI : GlazierNumericField_uGUI, ISleekUInt16Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    private ushort _state;

    public ushort state
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

    public ushort minValue { get; set; }

    public ushort maxValue { get; set; } = ushort.MaxValue;


    public event TypedUInt16 onTypedUInt16;

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
            _state = MathfEx.Clamp(_state, minValue, maxValue);
            this.onTypedUInt16?.Invoke(this, _state);
            return true;
        }
        return false;
    }

    protected override string NumberToString()
    {
        return state.ToString();
    }
}
