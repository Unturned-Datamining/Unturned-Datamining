using Steamworks;

namespace SDG.Unturned;

public delegate void RepairVehicleRequestHandler(CSteamID instigatorSteamID, InteractableVehicle vehicle, ref ushort pendingTotalHealing, ref bool shouldAllow);
