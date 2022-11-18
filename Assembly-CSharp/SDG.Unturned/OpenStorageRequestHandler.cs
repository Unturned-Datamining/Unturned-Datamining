using Steamworks;

namespace SDG.Unturned;

public delegate void OpenStorageRequestHandler(CSteamID instigator, InteractableStorage storage, ref bool shouldAllow);
