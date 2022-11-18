using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public delegate void Hurt(Player player, byte damage, Vector3 force, EDeathCause cause, ELimb limb, CSteamID killer);
