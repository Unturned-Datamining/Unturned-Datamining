using UnityEngine.UIElements;

namespace SDG.Unturned;

internal abstract class GlazierNumericField_UIToolkit : GlazierStringField_UIToolkit, ISleekNumericField, ISleekWithTooltip
{
    public GlazierNumericField_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
    }

    protected void SynchronizeText()
    {
        base.Text = NumberToString();
    }

    protected override void OnControlValueChanged(ChangeEvent<string> changeEvent)
    {
        if (!ParseNumericInput(changeEvent.newValue))
        {
            SynchronizeText();
        }
    }

    protected abstract bool ParseNumericInput(string input);

    protected abstract string NumberToString();
}
