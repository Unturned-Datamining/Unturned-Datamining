namespace SDG.Unturned;

public delegate void TakeItemRequestHandler(Player player, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page, ItemData itemData, ref bool shouldAllow);
