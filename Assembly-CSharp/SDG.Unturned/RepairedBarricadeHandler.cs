using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public delegate void RepairedBarricadeHandler(CSteamID instigatorSteamID, Transform barricadeTransform, float totalHealing);
