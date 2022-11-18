using UnityEngine;

namespace SDG.Unturned;

public delegate void DeployBarricadeRequestHandler(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow);
