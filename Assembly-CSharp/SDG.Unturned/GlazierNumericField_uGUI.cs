namespace SDG.Unturned;

internal abstract class GlazierNumericField_uGUI : GlazierStringField_uGUI, ISleekNumericField, ISleekWithTooltip
{
    public GlazierNumericField_uGUI(Glazier_uGUI glazier)
        : base(glazier)
    {
    }

    protected void SynchronizeText()
    {
        base.text = NumberToString();
    }

    protected override void OnUnityValueChanged(string input)
    {
        if (!ParseNumericInput(input))
        {
            SynchronizeText();
        }
    }

    protected abstract bool ParseNumericInput(string input);

    protected abstract string NumberToString();
}
