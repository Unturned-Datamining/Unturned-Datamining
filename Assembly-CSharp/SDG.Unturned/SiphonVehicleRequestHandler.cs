namespace SDG.Unturned;

public delegate void SiphonVehicleRequestHandler(InteractableVehicle vehicle, Player instigatingPlayer, ref bool shouldAllow, ref ushort desiredAmount);
