using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public delegate void DamageStructureRequestHandler(CSteamID instigatorSteamID, Transform structureTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin);
