using System;

namespace SDG.Unturned;

public class NewsStatusData
{
    public bool Allow_Workshop_News;

    public DateTime Featured_Workshop_Start;

    public DateTime Featured_Workshop_End;

    public ulong[] Featured_Workshop_File_Ids;

    public EMapStatus Featured_Workshop_Status;

    public EFeaturedWorkshopType Featured_Workshop_Type;

    public bool Featured_Workshop_Auto_Expand_Description;

    public string Featured_Workshop_Override_Description;

    public string Featured_Workshop_Link_Text;

    public string Featured_Workshop_Link_URL;

    public int[] Featured_Workshop_Associated_Stockpile_Items;

    public uint Popular_Workshop_Trend_Days;

    public int Popular_Workshop_Carousel_Items;

    public int Announcements_Count;

    public bool isFeatured(ulong fileId)
    {
        if (fileId == 0L || Featured_Workshop_File_Ids == null)
        {
            return false;
        }
        ulong[] featured_Workshop_File_Ids = Featured_Workshop_File_Ids;
        for (int i = 0; i < featured_Workshop_File_Ids.Length; i++)
        {
            if (featured_Workshop_File_Ids[i] == fileId)
            {
                return true;
            }
        }
        return false;
    }

    public NewsStatusData()
    {
        Allow_Workshop_News = true;
        Featured_Workshop_File_Ids = new ulong[0];
        Featured_Workshop_Associated_Stockpile_Items = new int[0];
        Popular_Workshop_Trend_Days = 30u;
        Popular_Workshop_Carousel_Items = 3;
        Announcements_Count = 3;
    }
}
