using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public class ItemStoreLiveConfig
{
    public long promotionId;

    public int[] newItems;

    public int[] featuredItems;

    public int[] excludeItemsFromHighlight;

    public string saleTitle;

    public DateTime saleStart;

    public DateTime saleEnd;

    public void Parse(DatDictionary data)
    {
        promotionId = data.ParseInt64("PromotionId", 0L);
        if (data.TryGetList("NewItems", out var node))
        {
            List<int> list = new List<int>(node.Count);
            foreach (DatValue value4 in node.GetValues())
            {
                if (value4.TryParseInt32(out var value))
                {
                    list.Add(value);
                }
            }
            newItems = list.ToArray();
        }
        else
        {
            newItems = new int[0];
        }
        if (data.TryGetList("FeaturedItems", out var node2))
        {
            List<int> list2 = new List<int>(node2.Count);
            foreach (DatValue value5 in node2.GetValues())
            {
                if (value5.TryParseInt32(out var value2))
                {
                    list2.Add(value2);
                }
            }
            featuredItems = list2.ToArray();
        }
        else
        {
            featuredItems = new int[0];
        }
        if (data.TryGetList("ExcludeItemsFromHighlight", out var node3))
        {
            List<int> list3 = new List<int>(node3.Count);
            foreach (DatValue value6 in node3.GetValues())
            {
                if (value6.TryParseInt32(out var value3))
                {
                    list3.Add(value3);
                }
            }
            excludeItemsFromHighlight = list3.ToArray();
        }
        else
        {
            excludeItemsFromHighlight = new int[0];
        }
        saleTitle = data.GetString("SaleTitle");
        saleStart = data.ParseDateTimeUtc("SaleStart");
        saleEnd = data.ParseDateTimeUtc("SaleEnd");
    }
}
