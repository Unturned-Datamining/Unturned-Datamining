using System;
using UnityEngine;

namespace SDG.Unturned;

public interface ISleekSprite : ISleekElement
{
    Sprite Sprite { get; set; }

    SleekColor TintColor { get; set; }

    ESleekSpriteType DrawMethod { get; set; }

    bool IsRaycastTarget { get; set; }

    Vector2Int TileRepeatHintForUITK { get; set; }

    event Action OnClicked;
}
