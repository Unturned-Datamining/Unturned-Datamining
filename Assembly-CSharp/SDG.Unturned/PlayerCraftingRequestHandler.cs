namespace SDG.Unturned;

public delegate void PlayerCraftingRequestHandler(PlayerCrafting crafting, ref ushort itemID, ref byte blueprintIndex, ref bool shouldAllow);
