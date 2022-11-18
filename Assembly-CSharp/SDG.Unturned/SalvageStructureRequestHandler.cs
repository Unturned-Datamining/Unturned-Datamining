using System;
using Steamworks;

namespace SDG.Unturned;

[Obsolete]
public delegate void SalvageStructureRequestHandler(CSteamID steamID, byte x, byte y, ushort index, ref bool shouldAllow);
