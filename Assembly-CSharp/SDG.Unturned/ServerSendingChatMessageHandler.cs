using UnityEngine;

namespace SDG.Unturned;

public delegate void ServerSendingChatMessageHandler(ref string text, ref Color color, SteamPlayer fromPlayer, SteamPlayer toPlayer, EChatMode mode, ref string iconURL, ref bool useRichTextFormatting);
