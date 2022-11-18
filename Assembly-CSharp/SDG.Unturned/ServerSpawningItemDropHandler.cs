using UnityEngine;

namespace SDG.Unturned;

public delegate void ServerSpawningItemDropHandler(Item item, ref Vector3 location, ref bool shouldAllow);
