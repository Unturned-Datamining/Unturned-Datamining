using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public delegate void TransformStructureRequestHandler(CSteamID instigator, byte x, byte y, uint instanceID, ref Vector3 point, ref byte angle_x, ref byte angle_y, ref byte angle_z, ref bool shouldAllow);
