using UnityEngine;

namespace SDG.Unturned;

public interface ISleekElement
{
    bool isVisible { get; set; }

    ISleekElement parent { get; }

    ISleekLabel sideLabel { get; }

    int positionOffset_X { get; set; }

    int positionOffset_Y { get; set; }

    float positionScale_X { get; set; }

    float positionScale_Y { get; set; }

    int sizeOffset_X { get; set; }

    int sizeOffset_Y { get; set; }

    float sizeScale_X { get; set; }

    float sizeScale_Y { get; set; }

    ISleekElement attachmentRoot { get; }

    bool isAnimatingTransform { get; }

    void destroy();

    void lerpPositionOffset(int newPositionOffset_X, int newPositionOffset_Y, ESleekLerp lerp, float time);

    void lerpPositionScale(float newPositionScale_X, float newPositionScale_Y, ESleekLerp lerp, float time);

    void lerpSizeOffset(int newSizeOffset_X, int newSizeOffset_Y, ESleekLerp lerp, float time);

    void lerpSizeScale(float newSizeScale_X, float newSizeScale_Y, ESleekLerp lerp, float time);

    void AddChild(ISleekElement child);

    void addLabel(string text, ESleekSide side);

    void addLabel(string text, Color color, ESleekSide side);

    void updateLabel(string text);

    int FindIndexOfChild(ISleekElement sleek);

    ISleekElement GetChildAtIndex(int index);

    void Update();

    void RemoveChild(ISleekElement child);

    void RemoveAllChildren();

    Vector2 ViewportToNormalizedPosition(Vector2 viewportPosition);

    Vector2 GetNormalizedCursorPosition();
}
