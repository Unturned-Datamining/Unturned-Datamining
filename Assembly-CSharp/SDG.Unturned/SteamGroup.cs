using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class SteamGroup
{
    private CSteamID _steamID;

    private string _name;

    private Texture2D _icon;

    public CSteamID steamID => _steamID;

    public string name => _name;

    public Texture2D icon => _icon;

    public SteamGroup(CSteamID newSteamID, string newName, Texture2D newIcon)
    {
        _steamID = newSteamID;
        _name = newName;
        _icon = newIcon;
    }
}
