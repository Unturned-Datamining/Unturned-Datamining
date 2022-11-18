using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public delegate void RepairStructureRequestHandler(CSteamID instigatorSteamID, Transform structureTransform, ref float pendingTotalHealing, ref bool shouldAllow);
