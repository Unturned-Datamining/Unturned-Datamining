using UnityEngine;

namespace SDG.Unturned;

public class SleekNew : SleekWrapper
{
    internal ISleekLabel label;

    public SleekNew(bool isUpdate = false)
    {
        base.PositionOffset_X = -105f;
        base.PositionScale_X = 1f;
        base.SizeOffset_X = 100f;
        base.SizeOffset_Y = 30f;
        label = Glazier.Get().CreateLabel();
        label.SizeScale_X = 1f;
        label.SizeScale_Y = 1f;
        label.TextAlignment = TextAnchor.MiddleRight;
        label.Text = Provider.localization.format(isUpdate ? "Updated" : "New");
        label.TextColor = Color.green;
        label.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        AddChild(label);
    }
}
