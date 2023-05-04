using UnityEngine;

namespace SDG.Unturned;

public interface ISleekLabel : ISleekElement
{
    string text { get; set; }

    FontStyle fontStyle { get; set; }

    TextAnchor fontAlignment { get; set; }

    ESleekFontSize fontSize { get; set; }

    ETextContrastContext shadowStyle { get; set; }

    SleekColor textColor { get; set; }

    bool enableRichText { get; set; }
}
