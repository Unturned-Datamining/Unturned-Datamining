using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public delegate void DamageToolPlayerDamagedHandler(Player player, ref EDeathCause cause, ref ELimb limb, ref CSteamID killer, ref Vector3 direction, ref float damage, ref float times, ref bool canDamage);
