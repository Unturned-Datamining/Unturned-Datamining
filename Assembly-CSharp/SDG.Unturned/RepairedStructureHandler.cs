using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public delegate void RepairedStructureHandler(CSteamID instigatorSteamID, Transform structureTransform, float totalHealing);
