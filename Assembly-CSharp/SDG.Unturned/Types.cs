using System;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class Types
{
    public static readonly Type STRING_TYPE = typeof(string);

    public static readonly Type STRING_ARRAY_TYPE = typeof(string[]);

    public static readonly Type BOOLEAN_TYPE = typeof(bool);

    public static readonly Type BOOLEAN_ARRAY_TYPE = typeof(bool[]);

    public static readonly Type BYTE_ARRAY_TYPE = typeof(byte[]);

    public static readonly Type BYTE_TYPE = typeof(byte);

    public static readonly Type INT16_TYPE = typeof(short);

    public static readonly Type UINT16_TYPE = typeof(ushort);

    public static readonly Type INT32_ARRAY_TYPE = typeof(int[]);

    public static readonly Type INT32_TYPE = typeof(int);

    public static readonly Type UINT32_TYPE = typeof(uint);

    public static readonly Type SINGLE_TYPE = typeof(float);

    public static readonly Type INT64_TYPE = typeof(long);

    public static readonly Type UINT64_ARRAY_TYPE = typeof(ulong[]);

    public static readonly Type UINT64_TYPE = typeof(ulong);

    public static readonly Type STEAM_ID_TYPE = typeof(CSteamID);

    public static readonly Type GUID_TYPE = typeof(Guid);

    public static readonly Type VECTOR3_TYPE = typeof(Vector3);

    public static readonly Type COLOR_TYPE = typeof(Color);

    public static readonly Type QUATERNION_TYPE = typeof(Quaternion);

    public static readonly byte[] SHIFTS = new byte[8] { 1, 2, 4, 8, 16, 32, 64, 128 };
}
