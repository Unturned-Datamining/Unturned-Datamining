using TMPro;

namespace SDG.Unturned;

internal class GlazierUInt32Field_uGUI : GlazierNumericField_uGUI, ISleekUInt32Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    private uint _state;

    public uint state
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

    public event TypedUInt32 onTypedUInt32;

    public GlazierUInt32Field_uGUI(Glazier_uGUI glazier)
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
        if (uint.TryParse(input, out _state))
        {
            this.onTypedUInt32?.Invoke(this, _state);
            return true;
        }
        return false;
    }

    protected override string NumberToString()
    {
        return state.ToString();
    }
}
