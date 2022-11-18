namespace SDG.Unturned;

public class CuratedMapLink
{
    public string Name;

    public ulong Workshop_File_Id;

    public ulong[] Required_Workshop_File_Ids = new ulong[0];

    public bool Visible_In_Singleplayer_Recommendations_List;
}
