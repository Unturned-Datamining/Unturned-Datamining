using System;
using UnityEngine;

namespace SDG.Unturned;

public interface ISleekScrollView : ISleekElement
{
    bool scaleContentToWidth { get; set; }

    bool scaleContentToHeight { get; set; }

    float contentScaleFactor { get; set; }

    bool reduceWidthWhenScrollbarVisible { get; set; }

    Vector2 contentSizeOffset { get; set; }

    Vector2 state { get; set; }

    Vector2 normalizedStateCenter { get; set; }

    bool handleScrollWheel { get; set; }

    SleekColor backgroundColor { get; set; }

    SleekColor foregroundColor { get; set; }

    float normalizedVerticalPosition { get; }

    float normalizedViewportHeight { get; }

    event Action<Vector2> onValueChanged;

    void ScrollToBottom();
}
