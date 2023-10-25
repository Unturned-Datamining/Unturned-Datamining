namespace SDG.Unturned;

/// <summary>
/// Tags how client expects server to use a raycast input.
/// For example, client may think they fired a gun while server thinks they dequipped the gun,
/// so tagging the input prevents the server from handling it as a punch instead.
/// </summary>
public enum ERaycastInfoUsage
{
    Punch,
    ConsumeableAid,
    Melee,
    Gun,
    Bayonet,
    Refill,
    Tire,
    Battery,
    Detonator,
    Carlockpick,
    Fuel,
    Carjack,
    Grower,
    ArrestStart,
    ArrestEnd
}
