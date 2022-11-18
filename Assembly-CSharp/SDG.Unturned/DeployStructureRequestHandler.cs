using UnityEngine;

namespace SDG.Unturned;

public delegate void DeployStructureRequestHandler(Structure structure, ItemStructureAsset asset, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow);
