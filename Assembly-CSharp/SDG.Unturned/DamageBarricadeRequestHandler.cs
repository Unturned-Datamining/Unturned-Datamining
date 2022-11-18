using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public delegate void DamageBarricadeRequestHandler(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin);
