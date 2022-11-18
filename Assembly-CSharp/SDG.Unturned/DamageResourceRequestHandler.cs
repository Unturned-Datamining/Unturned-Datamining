using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public delegate void DamageResourceRequestHandler(CSteamID instigatorSteamID, Transform objectTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin);
