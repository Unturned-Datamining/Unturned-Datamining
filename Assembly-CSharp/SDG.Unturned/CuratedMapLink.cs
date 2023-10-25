namespace SDG.Unturned;

public class CuratedMapLink
{
    /// <summary>
    /// Folder name of the map when it was in the game.
    /// </summary>
    public string Name;

    /// <summary>
    /// Published steam id for the file after it was moved to the workshop.
    /// </summary>
    public ulong Workshop_File_Id;

    /// <summary>
    /// Dependencies to subscribe to when subscribing through the in-game menu.
    /// e.g. Hawaii's assets are stored separately on the workshop.
    /// </summary>
    public ulong[] Required_Workshop_File_Ids = new ulong[0];

    /// <summary>
    /// Only applies if player is not subscribed to the workshop file.
    /// Should an advertisement be shown in the Menu &gt; Singleplayer &gt; Curated list?
    /// </summary>
    public bool Visible_In_Singleplayer_Recommendations_List;
}
