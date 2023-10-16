using System;
using UnityEngine;

namespace SDG.Unturned;

public interface ISleekScrollView : ISleekElement
{
    bool ScaleContentToWidth { get; set; }

    bool ScaleContentToHeight { get; set; }

    float ContentScaleFactor { get; set; }

    bool ReduceWidthWhenScrollbarVisible { get; set; }

    ESleekScrollbarVisibility VerticalScrollbarVisibility { get; set; }

    Vector2 ContentSizeOffset { get; set; }

    Vector2 NormalizedStateCenter { get; set; }

    bool HandleScrollWheel { get; set; }

    SleekColor BackgroundColor { get; set; }

    SleekColor ForegroundColor { get; set; }

    float NormalizedVerticalPosition { get; }

    float NormalizedViewportHeight { get; }

    bool ContentUseManualLayout { get; set; }

    bool AlignContentToBottom { get; set; }

    bool IsRaycastTarget { get; set; }

    event Action<Vector2> OnNormalizedValueChanged;

    void ScrollToTop();

    void ScrollToBottom();
}
