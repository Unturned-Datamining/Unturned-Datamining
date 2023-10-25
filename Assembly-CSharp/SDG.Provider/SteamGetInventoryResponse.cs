using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SDG.Provider;

/// <summary>
/// Response data from IInventoryService GetInventory web API.
///
/// One player's inventory became so large that the Steam client's built-in GetInventory fails,
/// so as temporary fix we can send them a json file with their inventory.
/// </summary>
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
        /// <summary>
        /// Json string representation of the contained items.
        /// </summary>
        public string item_json;
    }

    public InnerResponse response;

    /// <summary>
    /// Parse response from json file.
    /// </summary>
    public static List<Item> parse(string path)
    {
        using StreamReader reader = new StreamReader(path);
        using JsonReader reader2 = new JsonTextReader(reader);
        return JsonConvert.DeserializeObject<List<Item>>(new JsonSerializer().Deserialize<SteamGetInventoryResponse>(reader2).response.item_json);
    }
}
