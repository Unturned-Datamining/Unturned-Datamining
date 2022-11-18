using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public delegate void DamageObjectRequestHandler(CSteamID instigatorSteamID, Transform objectTransform, byte section, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin);
