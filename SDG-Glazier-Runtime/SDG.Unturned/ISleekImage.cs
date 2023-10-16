using System;
using UnityEngine;

namespace SDG.Unturned;

public interface ISleekImage : ISleekElement
{
    Texture Texture { get; set; }

    float RotationAngle { get; set; }

    bool CanRotate { get; set; }

    bool ShouldDestroyTexture { get; set; }

    SleekColor TintColor { get; set; }

    event Action OnClicked;

    event Action OnRightClicked;

    void UpdateTexture(Texture2D newTexture);

    void SetTextureAndShouldDestroy(Texture2D texture, bool shouldDestroyTexture);
}
