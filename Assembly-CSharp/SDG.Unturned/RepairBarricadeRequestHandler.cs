using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public delegate void RepairBarricadeRequestHandler(CSteamID instigatorSteamID, Transform barricadeTransform, ref float pendingTotalHealing, ref bool shouldAllow);
