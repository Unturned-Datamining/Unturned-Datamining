using UnityEngine;

namespace SDG.Unturned;

public class SleekNew : SleekWrapper
{
    private ISleekLabel label;

    public SleekNew(bool isUpdate = false)
    {
        base.positionOffset_X = -105;
        base.positionScale_X = 1f;
        base.sizeOffset_X = 100;
        base.sizeOffset_Y = 30;
        label = Glazier.Get().CreateLabel();
        label.sizeScale_X = 1f;
        label.sizeScale_Y = 1f;
        label.fontAlignment = TextAnchor.MiddleRight;
        label.text = Provider.localization.format(isUpdate ? "Updated" : "New");
        label.textColor = Color.green;
        label.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        AddChild(label);
    }
}
