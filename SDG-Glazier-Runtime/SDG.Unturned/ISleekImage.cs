using System;
using UnityEngine;

namespace SDG.Unturned;

public interface ISleekImage : ISleekElement
{
    Texture texture { get; set; }

    float angle { get; set; }

    bool isAngled { get; set; }

    bool shouldDestroyTexture { get; set; }

    SleekColor color { get; set; }

    event Action onImageClicked;

    event Action onImageRightClicked;

    void updateTexture(Texture2D newTexture);

    void setTextureAndShouldDestroy(Texture2D texture, bool shouldDestroyTexture);
}
