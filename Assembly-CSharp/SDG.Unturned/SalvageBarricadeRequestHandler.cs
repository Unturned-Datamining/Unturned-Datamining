using System;
using Steamworks;

namespace SDG.Unturned;

[Obsolete]
public delegate void SalvageBarricadeRequestHandler(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ref bool shouldAllow);
