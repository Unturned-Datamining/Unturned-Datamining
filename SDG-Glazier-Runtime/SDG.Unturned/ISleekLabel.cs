using UnityEngine;

namespace SDG.Unturned;

public interface ISleekLabel : ISleekElement
{
    string Text { get; set; }

    FontStyle FontStyle { get; set; }

    TextAnchor TextAlignment { get; set; }

    ESleekFontSize FontSize { get; set; }

    ETextContrastContext TextContrastContext { get; set; }

    SleekColor TextColor { get; set; }

    bool AllowRichText { get; set; }
}
