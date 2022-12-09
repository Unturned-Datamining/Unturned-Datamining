using System;

namespace SDG.Unturned;

public class StockpileStatusData
{
    public DateTime New_Items_Start;

    public DateTime New_Items_End;

    public int[] New_Items;

    public int[] Featured_Items;

    public int[] Exclude_Items_From_Highlight;

    public StockpileStatusData()
    {
        New_Items = new int[0];
        Featured_Items = new int[0];
        Exclude_Items_From_Highlight = new int[0];
    }
}
