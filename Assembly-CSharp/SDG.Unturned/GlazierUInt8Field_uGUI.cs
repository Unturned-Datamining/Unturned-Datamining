using TMPro;

namespace SDG.Unturned;

internal class GlazierUInt8Field_uGUI : GlazierNumericField_uGUI, ISleekUInt8Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    private byte _state;

    public byte state
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

    public event TypedByte onTypedByte;

    public GlazierUInt8Field_uGUI(Glazier_uGUI glazier)
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
        if (byte.TryParse(input, out _state))
        {
            this.onTypedByte?.Invoke(this, _state);
            return true;
        }
        return false;
    }

    protected override string NumberToString()
    {
        return state.ToString();
    }
}
