using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SDG.Provider;

public class SteamGetInventoryResponse
{
    public class Item
    {
        public ulong itemid;

        public ushort quantity;

        public int itemdefid;
    }

    public class InnerResponse
    {
        public string item_json;
    }

    public InnerResponse response;

    public static List<Item> parse(string path)
    {
        using StreamReader reader = new StreamReader(path);
        using JsonReader reader2 = new JsonTextReader(reader);
        return JsonConvert.DeserializeObject<List<Item>>(new JsonSerializer().Deserialize<SteamGetInventoryResponse>(reader2).response.item_json);
    }
}
