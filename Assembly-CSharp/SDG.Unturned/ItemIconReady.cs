using UnityEngine;

namespace SDG.Unturned;

/// <param name="handle">Matches handle returned by request, or -1 if cached.</param>
public delegate void ItemIconReady(int handle, Texture2D texture);
