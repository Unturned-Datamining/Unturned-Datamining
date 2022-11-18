using TMPro;

namespace SDG.Unturned;

internal class GlazierInt32Field_uGUI : GlazierNumericField_uGUI, ISleekInt32Field, ISleekElement, ISleekNumericField, ISleekWithTooltip
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
            SynchronizeText();
        }
    }

    public event TypedInt32 onTypedInt;

    public GlazierInt32Field_uGUI(Glazier_uGUI glazier)
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
        if (int.TryParse(input, out _state))
        {
            this.onTypedInt?.Invoke(this, _state);
            return true;
        }
        return false;
    }

    protected override string NumberToString()
    {
        return state.ToString();
    }
}
