using UnityEngine;

namespace SDG.Unturned;

public interface ISleekElement
{
    bool IsVisible { get; set; }

    ISleekElement Parent { get; }

    ISleekLabel SideLabel { get; }

    float PositionOffset_X { get; set; }

    float PositionOffset_Y { get; set; }

    float PositionScale_X { get; set; }

    float PositionScale_Y { get; set; }

    float SizeOffset_X { get; set; }

    float SizeOffset_Y { get; set; }

    float SizeScale_X { get; set; }

    float SizeScale_Y { get; set; }

    ISleekElement AttachmentRoot { get; }

    bool IsAnimatingTransform { get; }

    bool UseManualLayout { get; set; }

    bool UseWidthLayoutOverride { get; set; }

    bool UseHeightLayoutOverride { get; set; }

    ESleekChildLayout UseChildAutoLayout { get; set; }

    ESleekChildPerpendicularAlignment ChildPerpendicularAlignment { get; set; }

    bool ExpandChildren { get; set; }

    bool IgnoreLayout { get; set; }

    float ChildAutoLayoutPadding { get; set; }

    void InternalDestroy();

    void AnimatePositionOffset(float newPositionOffset_X, float newPositionOffset_Y, ESleekLerp lerp, float time);

    void AnimatePositionScale(float newPositionScale_X, float newPositionScale_Y, ESleekLerp lerp, float time);

    void AnimateSizeOffset(float newSizeOffset_X, float newSizeOffset_Y, ESleekLerp lerp, float time);

    void AnimateSizeScale(float newSizeScale_X, float newSizeScale_Y, ESleekLerp lerp, float time);

    void AddChild(ISleekElement child);

    void AddLabel(string text, ESleekSide side);

    void AddLabel(string text, Color color, ESleekSide side);

    void UpdateLabel(string text);

    int FindIndexOfChild(ISleekElement sleek);

    ISleekElement GetChildAtIndex(int index);

    void Update();

    void RemoveChild(ISleekElement child);

    void RemoveAllChildren();

    Vector2 ViewportToNormalizedPosition(Vector2 viewportPosition);

    Vector2 GetNormalizedCursorPosition();

    Vector2 GetAbsoluteSize();

    void SetAsFirstSibling();
}
