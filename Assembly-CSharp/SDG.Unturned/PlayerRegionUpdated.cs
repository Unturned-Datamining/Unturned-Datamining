namespace SDG.Unturned;

public delegate void PlayerRegionUpdated(Player player, byte old_x, byte old_y, byte new_x, byte new_y, byte index, ref bool canIncrementIndex);
