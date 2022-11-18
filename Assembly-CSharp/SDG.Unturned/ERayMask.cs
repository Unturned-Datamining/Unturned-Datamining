using System;

namespace SDG.Unturned;

[Flags]
public enum ERayMask
{
    DEFAULT = 1,
    TRANSPARENT_FX = 2,
    IGNORE_RAYCAST = 4,
    BUILTIN_3 = 8,
    WATER = 0x10,
    UI = 0x20,
    BUILTIN_6 = 0x40,
    BUILTIN_7 = 0x80,
    LOGIC = 0x100,
    PLAYER = 0x200,
    ENEMY = 0x400,
    VIEWMODEL = 0x800,
    DEBRIS = 0x1000,
    ITEM = 0x2000,
    RESOURCE = 0x4000,
    LARGE = 0x8000,
    MEDIUM = 0x10000,
    SMALL = 0x20000,
    SKY = 0x40000,
    ENVIRONMENT = 0x80000,
    GROUND = 0x100000,
    CLIP = 0x200000,
    NAVMESH = 0x400000,
    ENTITY = 0x800000,
    AGENT = 0x1000000,
    LADDER = 0x2000000,
    VEHICLE = 0x4000000,
    BARRICADE = 0x8000000,
    STRUCTURE = 0x10000000,
    TIRE = 0x20000000,
    TRAP = 0x40000000,
    GROUND2 = int.MinValue
}
