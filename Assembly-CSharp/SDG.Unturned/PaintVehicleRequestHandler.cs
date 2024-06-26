using UnityEngine;

namespace SDG.Unturned;

public delegate void PaintVehicleRequestHandler(InteractableVehicle vehicle, Player instigatingPlayer, ref bool shouldAllow, ref Color32 desiredColor);
