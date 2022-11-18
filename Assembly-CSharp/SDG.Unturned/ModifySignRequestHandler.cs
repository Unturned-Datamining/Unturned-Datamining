using Steamworks;

namespace SDG.Unturned;

public delegate void ModifySignRequestHandler(CSteamID instigator, InteractableSign sign, ref string text, ref bool shouldAllow);
