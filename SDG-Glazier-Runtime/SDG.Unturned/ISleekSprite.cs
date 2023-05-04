using System;
using UnityEngine;

namespace SDG.Unturned;

public interface ISleekSprite : ISleekElement
{
    Sprite sprite { get; set; }

    SleekColor color { get; set; }

    ESleekSpriteType drawMethod { get; set; }

    bool isRaycastTarget { get; set; }

    event Action onImageClicked;
}
