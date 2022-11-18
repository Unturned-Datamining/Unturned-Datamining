using Steamworks;

namespace SDG.Unturned;

public delegate void DamageTireRequestHandler(CSteamID instigatorSteamID, InteractableVehicle vehicle, int tireIndex, ref bool shouldAllow, EDamageOrigin damageOrigin);
