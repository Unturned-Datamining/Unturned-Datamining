using UnityEngine;

namespace SDG.Unturned;

public delegate void Chatted(SteamPlayer player, EChatMode mode, ref Color chatted, ref bool isRich, string text, ref bool isVisible);
