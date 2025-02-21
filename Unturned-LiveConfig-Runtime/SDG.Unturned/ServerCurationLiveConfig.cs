using System.Collections.Generic;

namespace SDG.Unturned;

public class ServerCurationLiveConfig
{
    public ServerCurationLiveConfigItem[] items = new ServerCurationLiveConfigItem[0];

    public void Parse(DatDictionary data)
    {
        if (data.TryGetList("Items", out var node))
        {
            List<ServerCurationLiveConfigItem> list = new List<ServerCurationLiveConfigItem>(node.Count);
            foreach (IDatNode item2 in node)
            {
                ServerCurationLiveConfigItem item = default(ServerCurationLiveConfigItem);
                if (item.TryParse(item2))
                {
                    list.Add(item);
                }
            }
            items = list.ToArray();
        }
        else
        {
            items = new ServerCurationLiveConfigItem[0];
        }
    }

    public bool IsRecommended(int id)
    {
        if (items != null)
        {
            ServerCurationLiveConfigItem[] array = items;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].id == id)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
