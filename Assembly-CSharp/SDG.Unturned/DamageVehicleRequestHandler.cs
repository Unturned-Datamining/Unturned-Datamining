using Steamworks;

namespace SDG.Unturned;

public delegate void DamageVehicleRequestHandler(CSteamID instigatorSteamID, InteractableVehicle vehicle, ref ushort pendingTotalDamage, ref bool canRepair, ref bool shouldAllow, EDamageOrigin damageOrigin);
